using System.Net.Http.Json;
using System.Text.Json;
using Kofi.Application.Services;

namespace Kofi.Infrastructure.Services;

// Talks to the card payment provider. Aspire tells HttpClient where it runs.
public class PaymentGatewayClient(HttpClient http) : IPaymentGatewayClient
{
    // The key names one payment attempt: a retried request with the same key
    // gets the same payment back, a new attempt needs a new key.
    public async Task<(Guid PaymentId, string RedirectUrl)> CreatePayment(
        decimal amount, string currency, string reference, string returnUrl, string idempotencyKey)
    {
        var response = await http.PostAsJsonAsync("/payments", new
        {
            amount,
            currency,
            reference,
            returnUrl,
            idempotencyKey,
        });
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        return (json.GetProperty("id").GetGuid(), json.GetProperty("redirectUrl").GetString()!);
    }

    // A saved card, charged with no customer present. Like CreatePayment, the
    // key names one attempt: a new attempt for the same order needs a new key.
    public async Task<(Guid PaymentId, PaymentStatus Status)> Charge(
        decimal amount, string currency, string reference, string paymentMethodToken, string idempotencyKey)
    {
        var response = await http.PostAsJsonAsync("/charges", new
        {
            amount,
            currency,
            reference,
            paymentMethodToken,
            idempotencyKey,
        });
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        return (json.GetProperty("id").GetGuid(), StatusOf(json));
    }

    // Stops a payment the customer has not made yet. One the customer made
    // in the meantime cannot be stopped any more, so its money goes back.
    // Returns the payment's final status: Cancelled, Refunded, or Declined
    // for an attempt that never took money.
    public async Task<PaymentStatus> CancelOrRefund(Guid paymentId)
    {
        var status = await Post($"/payments/{paymentId}/cancel");
        return status == PaymentStatus.Captured ? await Post($"/payments/{paymentId}/refund") : status;
    }

    private async Task<PaymentStatus> Post(string path)
    {
        var response = await http.PostAsync(path, null);

        response.EnsureSuccessStatusCode();
        return StatusOf(await response.Content.ReadFromJsonAsync<JsonElement>());
    }

    public async Task<PaymentStatus> GetStatus(Guid paymentId) => (await GetPayment(paymentId)).Status;

    public async Task<(PaymentStatus Status, string RedirectUrl)> GetPayment(Guid paymentId)
    {
        var json = await http.GetFromJsonAsync<JsonElement>($"/payments/{paymentId}");
        return (StatusOf(json), json.GetProperty("redirectUrl").GetString() ?? "");
    }

    // The customer authorises future charges on the provider's page; we only
    // ever charge the token the provider hands back for a succeeded setup.
    public async Task<(Guid SetupId, string RedirectUrl)> CreateCardSetup(
        string reference, string returnUrl, string idempotencyKey)
    {
        var response = await http.PostAsJsonAsync("/setups", new { reference, returnUrl, idempotencyKey });

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        return (json.GetProperty("id").GetGuid(), json.GetProperty("redirectUrl").GetString()!);
    }

    public async Task<(CardSetupStatus Status, string Reference, string? PaymentMethodToken, string? CardLabel)> GetCardSetup(Guid setupId)
    {
        var json = await http.GetFromJsonAsync<JsonElement>($"/setups/{setupId}");
        return (Enum.Parse<CardSetupStatus>(json.GetProperty("status").GetString()!),
            json.GetProperty("reference").GetString()!,
            json.GetProperty("paymentMethodToken").GetString(),
            json.GetProperty("cardLabel").GetString());
    }

    private static PaymentStatus StatusOf(JsonElement payment) =>
        Enum.Parse<PaymentStatus>(payment.GetProperty("status").GetString()!);
}
