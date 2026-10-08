using Kofi.Common.Abstractions;
using Kofi.Subscriptions.Contracts;
using Kofi.Subscriptions.Domain.Subscriptions;
using MediatR;

namespace Kofi.Subscriptions.Application.EventHandlers;

public class SubscriptionCancelledPublisher(IPublisher publisher) : IDomainEventHandler<SubscriptionCancelled>
{
    public Task Handle(SubscriptionCancelled cancelled, CancellationToken ct) =>
        publisher.Publish(new SubscriptionCancelledIntegrationEvent(cancelled.SubscriptionId), ct);
}
