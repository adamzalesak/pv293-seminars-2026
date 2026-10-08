using Kofi.Common.Abstractions;
using Kofi.Subscriptions.Contracts;
using Kofi.Subscriptions.Domain.Subscriptions;
using MediatR;

namespace Kofi.Subscriptions.Application.EventHandlers;

// The domain event stays inside the module; other modules hear the public fact.
public class SubscriptionActivatedPublisher(IPublisher publisher) : IDomainEventHandler<SubscriptionActivated>
{
    public Task Handle(SubscriptionActivated activated, CancellationToken ct) =>
        publisher.Publish(new SubscriptionActivatedIntegrationEvent(activated.SubscriptionId, activated.CustomerId,
            activated.ProductId, activated.ActivatedAt), ct);
}
