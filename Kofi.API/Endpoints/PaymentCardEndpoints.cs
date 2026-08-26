using Kofi.API.Auth;
using Microsoft.AspNetCore.Http.HttpResults;
using MediatR;
using Kofi.BusinessLayer.Features.Customers.ConfirmPaymentCardChange;
using Kofi.BusinessLayer.Features.Customers.GetPaymentCard;
using Kofi.BusinessLayer.Features.Customers.StartPaymentCardChange;

namespace Kofi.API.Endpoints;

// The signed-in customer's saved card, which all their subscriptions are charged to.
public static class PaymentCardEndpoints
{
    public static void MapPaymentCardEndpoints(this WebApplication app)
    {
        var card = app.MapGroup("/payment-card").WithTags("Payment card");

        card.MapGet("", async Task<Ok<PaymentCard>> (IMediator mediator) =>
            TypedResults.Ok(await mediator.Send(new GetPaymentCardQuery())))
            .RequireAuthorization(Roles.Customer);

        card.MapPost("/setup", async Task<Ok<StartPaymentCardChangeResponse>> (IMediator mediator, HttpRequest http) =>
        {
            // The provider sends the customer back here when they are done.
            var setup = await mediator.Send(new StartPaymentCardChangeCommand
            {
                ReturnUrl = $"{http.Scheme}://{http.Host}/payment-card/return",
            });
            return TypedResults.Ok(setup);
        }).RequireAuthorization(Roles.Customer);

        // Like an order's payment return: open, and trusting nothing but the
        // provider itself.
        card.MapGet("/return", async Task<Ok<ConfirmPaymentCardChangeResponse>> (IMediator mediator, Guid setupId) =>
            TypedResults.Ok(await mediator.Send(new ConfirmPaymentCardChangeCommand { SetupId = setupId })))
            .AllowAnonymous();
    }
}
