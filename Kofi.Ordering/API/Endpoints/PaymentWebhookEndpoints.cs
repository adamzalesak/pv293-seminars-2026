using System.Text.Json;
using Kofi.Common.Abstractions;
using Kofi.Common.Services;
using Kofi.Ordering.Application.Features.Orders.ConfirmOrderPayment;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Kofi.Ordering.API.Endpoints;

public static class PaymentWebhookEndpoints
{
    public static void MapPaymentWebhookEndpoints(this WebApplication app)
    {
        // The provider calls here after every decision about a payment; this,
        // not the customer's redirect, is where the shop learns the result.
        // The provider repeats a delivery until it gets a 2xx answer.
        app.MapPost("/payments/webhook", async Task<Results<Ok, UnauthorizedHttpResult>> (
                HttpRequest http, IPaymentWebhookVerifier verifier, IMediator mediator) =>
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
                    try
                    {
                        // Confirming is idempotent: it follows the order's state,
                        // so the redirect, the webhook and a repeated webhook
                        // agree on one outcome.
                        await mediator.Send(new ConfirmOrderPaymentCommand { Id = orderId, PaymentId = webhook.PaymentId });
                    }
                    catch (NotFoundException)
                    {
                        // A payment for an order the shop never saved: nothing to
                        // confirm.
                    }
                }

                return TypedResults.Ok();
            })
            .AllowAnonymous()
            .WithTags("Payments");
    }
}
