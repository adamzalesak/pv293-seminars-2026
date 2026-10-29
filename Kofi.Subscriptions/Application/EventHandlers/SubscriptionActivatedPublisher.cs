using Kofi.Common.Abstractions;
using Kofi.Subscriptions.Application.Persistence;
using Kofi.Subscriptions.Contracts;
using Kofi.Subscriptions.Domain.Subscriptions;
using Wolverine;

namespace Kofi.Subscriptions.Application.EventHandlers;

// The domain event stays inside the module; other modules hear the public fact.
public class SubscriptionActivatedPublisher(ISubscriptionsDbContext db, IMessageBus bus) : IDomainEventHandler<SubscriptionActivated>
{
    public async Task Handle(SubscriptionActivated activated, CancellationToken ct) =>
        await bus.PublishAsync(new SubscriptionActivatedIntegrationEvent(activated.SubscriptionId, activated.CustomerId,
            activated.ProductId, activated.ActivatedAt, await PublishedState.Of(db, activated.SubscriptionId, ct)));
}
