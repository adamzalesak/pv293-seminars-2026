using Kofi.Common.Abstractions;
using Kofi.Subscriptions.Contracts;
using Kofi.Subscriptions.Domain.Subscriptions;
using MediatR;

namespace Kofi.Subscriptions.Application.EventHandlers;

public class SubscriptionProductChangedPublisher(IPublisher publisher) : IDomainEventHandler<SubscriptionProductChanged>
{
    public Task Handle(SubscriptionProductChanged changed, CancellationToken ct) =>
        publisher.Publish(new SubscriptionProductChangedIntegrationEvent(changed.SubscriptionId, changed.ProductId), ct);
}
