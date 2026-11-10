using Kofi.Application.Features.Subscriptions.ActivateSubscription;
using Kofi.Application.Features.Subscriptions.AddDeliveryExtra;
using Kofi.Application.Features.Subscriptions.CancelSubscription;
using Kofi.Application.Features.Subscriptions.ChangePlan;
using Kofi.Application.Features.Subscriptions.GetSubscription;
using Kofi.Application.Features.Subscriptions.GetSubscriptions;
using Kofi.Application.Features.Subscriptions.PauseSubscription;
using Kofi.Application.Features.Subscriptions.PrepareDelivery;
using Kofi.Application.Features.Subscriptions.ProcessDueDeliveries;
using Kofi.Application.Features.Subscriptions.RemoveDeliveryExtra;
using Kofi.Application.Features.Subscriptions.ResumeSubscription;
using Kofi.Application.Features.Subscriptions.SkipDelivery;
using Kofi.Application.Features.Subscriptions.StartCardSetup;
using Kofi.Application.Features.Subscriptions.Subscribe;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Kofi.Endpoints;

public static class SubscriptionEndpoints
{
    public static void MapSubscriptionEndpoints(this WebApplication app)
    {
        var subscriptions = app.MapGroup("/subscriptions").WithTags("Subscriptions").RequireAuthorization();

        subscriptions.MapGet("", async Task<Ok<List<SubscriptionSummary>>> (IMediator mediator) =>
            TypedResults.Ok(await mediator.Send(new GetSubscriptionsQuery())));
        subscriptions.MapGet("/{id:guid}", async Task<Results<Ok<SubscriptionDetail>, NotFound>> (IMediator mediator, Guid id) =>
        {
            var detail = await mediator.Send(new GetSubscriptionQuery { Id = id });
            return detail == null ? TypedResults.NotFound() : TypedResults.Ok(detail);
        });
        subscriptions.MapPost("", async Task<Created<SubscribeResponse>> (IMediator mediator, SubscribeCommand command) =>
            {
                var subscription = await mediator.Send(command);
                return TypedResults.Created($"/subscriptions/{subscription.Id}", subscription);
            })
            .RequireAuthorization(Roles.Customer);

        // The customer authorises their card on the provider's page, then comes
        // back to activate the subscription with the setup the provider made.
        subscriptions.MapPost("/{id:guid}/card-setup", async Task<Ok<StartCardSetupResponse>> (
                IMediator mediator, Guid id, HttpRequest http) =>
            {
                // The provider sends the customer back here when they are done.
                var setup = await mediator.Send(new StartCardSetupCommand
                {
                    Id = id,
                    ReturnUrl = $"{http.Scheme}://{http.Host}/subscriptions/{id}/card-return",
                });

                return TypedResults.Ok(setup);
            })
            .RequireAuthorization(Roles.Customer);

        // A customer whose card is already saved activates straight away.
        subscriptions.MapPost("/{id:guid}/activate", async Task<Ok<ActivateSubscriptionResponse>> (IMediator mediator, Guid id) =>
                TypedResults.Ok(await mediator.Send(new ActivateSubscriptionCommand { Id = id })))
            .RequireAuthorization(Roles.Customer);

        // Like an order's payment return: open, and trusting nothing but the
        // provider itself.
        subscriptions.MapGet("/{id:guid}/card-return",
                async Task<Ok<ActivateSubscriptionResponse>> (IMediator mediator, Guid id, Guid setupId) =>
                    TypedResults.Ok(await mediator.Send(new ActivateSubscriptionCommand { Id = id, SetupId = setupId })))
            .AllowAnonymous();

        // Called once a day by the cron job in our infrastructure.
        subscriptions.MapPost("/process-due",
                async Task<Ok<ProcessDueDeliveriesResponse>> (DueDeliveryProcessor processor, CancellationToken ct) =>
                    TypedResults.Ok(await processor.Run(ct)))
            .RequireAuthorization(Roles.Administrator);

        subscriptions.MapPost("/{id:guid}/pause", async Task<Ok> (IMediator mediator, Guid id) =>
            {
                await mediator.Send(new PauseSubscriptionCommand { Id = id });
                return TypedResults.Ok();
            })
            .RequireAuthorization(Roles.Customer);

        subscriptions.MapPost("/{id:guid}/resume", async Task<Ok> (IMediator mediator, Guid id, ResumeSubscriptionRequest request) =>
            {
                await mediator.Send(new ResumeSubscriptionCommand { Id = id, NextDeliveryOn = request.NextDeliveryOn });
                return TypedResults.Ok();
            })
            .RequireAuthorization(Roles.Customer);

        subscriptions.MapPost("/{id:guid}/skip", async Task<Ok> (IMediator mediator, Guid id) =>
            {
                await mediator.Send(new SkipDeliveryCommand { Id = id });
                return TypedResults.Ok();
            })
            .RequireAuthorization(Roles.Customer);

        subscriptions.MapPost("/{id:guid}/cancel", async Task<Ok> (IMediator mediator, Guid id) =>
            {
                await mediator.Send(new CancelSubscriptionCommand { Id = id });
                return TypedResults.Ok();
            })
            .RequireAuthorization(Roles.Customer);

        subscriptions.MapPost("/{id:guid}/deliveries/{deliveryId:guid}/extras", async Task<Ok> (
                IMediator mediator, Guid id, Guid deliveryId, AddDeliveryExtraRequest request) =>
            {
                await mediator.Send(new AddDeliveryExtraCommand
                {
                    SubscriptionId = id,
                    DeliveryId = deliveryId,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                });

                return TypedResults.Ok();
            })
            .RequireAuthorization(Roles.Customer);
        subscriptions.MapDelete("/{id:guid}/deliveries/{deliveryId:guid}/extras/{productId:guid}", async Task<NoContent> (
                IMediator mediator, Guid id, Guid deliveryId, Guid productId) =>
            {
                await mediator.Send(new RemoveDeliveryExtraCommand
                {
                    SubscriptionId = id,
                    DeliveryId = deliveryId,
                    ProductId = productId,
                });

                return TypedResults.NoContent();
            })
            .RequireAuthorization(Roles.Customer);
        subscriptions.MapPost("/{id:guid}/deliveries/{deliveryId:guid}/prepare", async Task<Ok> (
                IMediator mediator, Guid id, Guid deliveryId) =>
            {
                await mediator.Send(new PrepareDeliveryCommand { SubscriptionId = id, DeliveryId = deliveryId });
                return TypedResults.Ok();
            })
            .RequireAuthorization(Roles.Administrator);

        subscriptions.MapPut("/{id:guid}/plan", async Task<NoContent> (IMediator mediator, Guid id, ChangePlanRequest request) =>
            {
                await mediator.Send(new ChangePlanCommand
                {
                    Id = id,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    IntervalDays = request.IntervalDays,
                });

                return TypedResults.NoContent();
            })
            .RequireAuthorization(Roles.Customer);
    }
}
