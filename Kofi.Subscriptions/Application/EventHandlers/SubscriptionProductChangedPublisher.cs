using Kofi.Common.Abstractions;
using Kofi.Subscriptions.Application.Persistence;
using Kofi.Subscriptions.Contracts;
using Kofi.Subscriptions.Domain.Subscriptions;
using Wolverine;

namespace Kofi.Subscriptions.Application.EventHandlers;

public class SubscriptionProductChangedPublisher(ISubscriptionsDbContext db, IMessageBus bus)
    : IDomainEventHandler<SubscriptionProductChanged>
{
    public async Task Handle(SubscriptionProductChanged changed, CancellationToken ct) =>
        await bus.PublishAsync(new SubscriptionProductChangedIntegrationEvent(changed.SubscriptionId, changed.ProductId,
            await PublishedState.Of(db, changed.SubscriptionId, ct)));
}
