using System.Text.Json;
using Kofi.Common.Services;
using Kofi.Ordering.Application.Payments;
using Microsoft.AspNetCore.Http.HttpResults;
using Wolverine;

namespace Kofi.Ordering.API.Endpoints;

public static class PaymentWebhookEndpoints
{
    public static void MapPaymentWebhookEndpoints(this WebApplication app)
    {
        // The provider calls here after every decision about a payment; this,
        // not the customer's redirect, is where the shop learns the result.
        // The provider repeats a delivery until it gets a 2xx answer, so the
        // answer comes only once the result is stored, and a repeat is
        // answered 2xx as well: refusing it would only make the provider retry.
        app.MapPost("/payments/webhook", async Task<Results<Ok, UnauthorizedHttpResult>> (
                HttpRequest http, IPaymentWebhookVerifier verifier, IMessageBus bus) =>
            {
                using var reader = new StreamReader(http.Body);
                var body = await reader.ReadToEndAsync();
                if (!verifier.IsAuthentic(body, http.Headers["X-Signature"]))
                {
                    return TypedResults.Unauthorized();
                }

                var webhook = JsonSerializer.Deserialize<PaymentWebhookRequest>(body, JsonSerializerOptions.Web)!;
                if (Guid.TryParse(webhook.Reference, out var orderId))
                {
                    // Stored in a durable queue before we answer.
                    await bus.PublishAsync(new PaymentResultReceived(webhook.EventId, webhook.PaymentId, orderId, webhook.Status));
                }

                return TypedResults.Ok();
            })
            .AllowAnonymous()
            .WithTags("Payments");
    }
}
