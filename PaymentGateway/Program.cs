using System.Globalization;
using System.Net;
using System.Text.Json.Serialization;
using PaymentGateway;

// Stand-in for an external card payment provider. See README.md.
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<PaymentStore>();
builder.Services.AddSingleton<SetupStore>();
builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

// Created by the shop's server; the customer is then sent to RedirectUrl.
app.MapPost("/payments", (CreatePaymentRequest request, PaymentStore store, HttpRequest http) =>
{
    var payment = store.GetOrCreate(request.IdempotencyKey, () => new Payment
    {
        Id = Guid.NewGuid(),
        Amount = request.Amount,
        Currency = request.Currency,
        Reference = request.Reference,
        ReturnUrl = request.ReturnUrl,
        CreatedAt = DateTime.UtcNow,
        Status = PaymentStatus.Created,
    });
    payment.RedirectUrl ??= $"{http.Scheme}://{http.Host}/pay/{payment.Id}";
    return Results.Ok(payment);
});

app.MapGet("/payments/{id:guid}", (Guid id, PaymentStore store) =>
    store.Find(id) is { } payment ? Results.Ok(payment) : Results.NotFound());

// The provider's payment page. Real card entry would happen here.
app.MapGet("/pay/{id:guid}", (Guid id, PaymentStore store) =>
{
    var payment = store.Find(id);
    if (payment == null)
    {
        return Results.NotFound();
    }
    var reference = WebUtility.HtmlEncode(payment.Reference);
    return Results.Content($"""
        <!doctype html>
        <html><head><meta charset="utf-8"><title>Payment</title></head>
        <body style="font-family: sans-serif; max-width: 28rem; margin: 3rem auto">
          <h1>Pay {payment.Amount.ToString("0.00", CultureInfo.InvariantCulture)} {payment.Currency}</h1>
          <p>Reference: {reference}</p>
          <p>Status: {payment.Status}</p>
          <form method="post" action="/pay/{payment.Id}/approve"><button>Pay</button></form>
          <form method="post" action="/pay/{payment.Id}/decline"><button>Decline</button></form>
        </body></html>
        """, "text/html");
});

app.MapPost("/pay/{id:guid}/approve", (Guid id, PaymentStore store) => Decide(id, store, PaymentStatus.Captured));
app.MapPost("/pay/{id:guid}/decline", (Guid id, PaymentStore store) => Decide(id, store, PaymentStatus.Declined));

// The same decisions without a browser, for kofi.http.
app.MapPost("/payments/{id:guid}/approve", (Guid id, PaymentStore store) => Settle(id, store, PaymentStatus.Captured));
app.MapPost("/payments/{id:guid}/decline", (Guid id, PaymentStore store) => Settle(id, store, PaymentStatus.Declined));

// The shop stops a payment the customer has not made yet, or returns money
// already taken. Anything else leaves the payment as it is.
app.MapPost("/payments/{id:guid}/cancel", (Guid id, PaymentStore store) =>
    store.Move(id, PaymentStatus.Created, PaymentStatus.Cancelled) is { } payment ? Results.Ok(payment) : Results.NotFound());
app.MapPost("/payments/{id:guid}/refund", (Guid id, PaymentStore store) =>
    store.Move(id, PaymentStatus.Captured, PaymentStatus.Refunded) is { } payment ? Results.Ok(payment) : Results.NotFound());

// A saved card, charged server to server with no customer present.
app.MapPost("/charges", async (ChargeRequest request, PaymentStore store) =>
{
    if (request.PaymentMethodToken == "tok_timeout")
    {
        await Task.Delay(TimeSpan.FromSeconds(30));
    }
    var payment = store.GetOrCreate(request.IdempotencyKey, () => new Payment
    {
        Id = Guid.NewGuid(),
        Amount = request.Amount,
        Currency = request.Currency,
        Reference = request.Reference,
        CreatedAt = DateTime.UtcNow,
        Status = request.PaymentMethodToken == "tok_declined" ? PaymentStatus.Declined : PaymentStatus.Captured,
    });
    return Results.Ok(payment);
});

// Saving a card for later payments: created by the shop's server, then the
// customer authorises it on the provider's page. The shop charges the saved
// card later through /charges.
app.MapPost("/setups", (CreateSetupRequest request, SetupStore store, HttpRequest http) =>
{
    var setup = store.GetOrCreate(request.IdempotencyKey, () => new CardSetup
    {
        Id = Guid.NewGuid(),
        Reference = request.Reference,
        ReturnUrl = request.ReturnUrl,
        CreatedAt = DateTime.UtcNow,
        Status = SetupStatus.Created,
    });
    setup.RedirectUrl ??= $"{http.Scheme}://{http.Host}/setup/{setup.Id}";
    return Results.Ok(setup);
});

app.MapGet("/setups/{id:guid}", (Guid id, SetupStore store) =>
    store.Find(id) is { } setup ? Results.Ok(setup) : Results.NotFound());

// The provider's page where the customer agrees to future charges. Real card
// entry would happen here; the test cards stand for what the bank will do.
app.MapGet("/setup/{id:guid}", (Guid id, SetupStore store) =>
{
    var setup = store.Find(id);
    if (setup == null)
    {
        return Results.NotFound();
    }
    var reference = WebUtility.HtmlEncode(setup.Reference);
    return Results.Content($"""
        <!doctype html>
        <html><head><meta charset="utf-8"><title>Save card</title></head>
        <body style="font-family: sans-serif; max-width: 28rem; margin: 3rem auto">
          <h1>Save a card for future payments</h1>
          <p>The shop may charge this card for future deliveries without asking you again. You can withdraw this by cancelling with the shop.</p>
          <p>Reference: {reference}</p>
          <p>Status: {setup.Status}</p>
          <form method="post" action="/setup/{setup.Id}/approve">
            <label>Test card
              <select name="token">
                <option value="tok_visa">Visa ending 4242 (payments approved)</option>
                <option value="tok_declined">Visa ending 0002 (payments declined)</option>
              </select>
            </label>
            <button>Authorise</button>
          </form>
          <form method="post" action="/setup/{setup.Id}/decline"><button>Decline</button></form>
        </body></html>
        """, "text/html");
});

app.MapPost("/setup/{id:guid}/approve", (Guid id, HttpRequest http, SetupStore store) =>
    Return(store.Decide(id, SetupStatus.Succeeded, http.Form["token"].FirstOrDefault() ?? "tok_visa")));
app.MapPost("/setup/{id:guid}/decline", (Guid id, SetupStore store) =>
    Return(store.Decide(id, SetupStatus.Declined, null)));

// The same decisions without a browser, for kofi.http; tok_visa unless a
// test card is named.
app.MapPost("/setups/{id:guid}/approve", (Guid id, string? token, SetupStore store) =>
    store.Decide(id, SetupStatus.Succeeded, token ?? "tok_visa") is { } setup ? Results.Ok(setup) : Results.NotFound());
app.MapPost("/setups/{id:guid}/decline", (Guid id, SetupStore store) =>
    store.Decide(id, SetupStatus.Declined, null) is { } setup ? Results.Ok(setup) : Results.NotFound());

app.MapGet("/", (PaymentStore store) =>
{
    var rows = string.Join("", store.All().Select(p =>
        $"<tr><td>{p.CreatedAt:u}</td><td>{WebUtility.HtmlEncode(p.Reference)}</td>" +
        $"<td>{p.Amount.ToString("0.00", CultureInfo.InvariantCulture)} {p.Currency}</td><td>{p.Status}</td></tr>"));
    return Results.Content($"""
        <!doctype html>
        <html><head><meta charset="utf-8"><title>Payments</title></head>
        <body style="font-family: sans-serif">
          <h1>Payments</h1>
          <table border="1" cellpadding="4">
            <tr><th>Created</th><th>Reference</th><th>Amount</th><th>Status</th></tr>{rows}
          </table>
        </body></html>
        """, "text/html");
});

app.Run();

static IResult Decide(Guid id, PaymentStore store, PaymentStatus status)
{
    var payment = store.Move(id, PaymentStatus.Created, status);
    if (payment == null || payment.ReturnUrl == null)
    {
        return Results.NotFound();
    }
    return Results.Redirect($"{payment.ReturnUrl}?paymentId={payment.Id}");
}

static IResult Return(CardSetup? setup) =>
    setup == null ? Results.NotFound() : Results.Redirect($"{setup.ReturnUrl}?setupId={setup.Id}");

static IResult Settle(Guid id, PaymentStore store, PaymentStatus status) =>
    store.Move(id, PaymentStatus.Created, status) is { } payment ? Results.Ok(payment) : Results.NotFound();
