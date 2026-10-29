using Kofi.Common.Abstractions;
using Kofi.Subscriptions.Application.Persistence;
using Kofi.Subscriptions.Contracts;
using Kofi.Subscriptions.Domain.Subscriptions;
using Wolverine;

namespace Kofi.Subscriptions.Application.EventHandlers;

public class SubscriptionCancelledPublisher(ISubscriptionsDbContext db, IMessageBus bus) : IDomainEventHandler<SubscriptionCancelled>
{
    public async Task Handle(SubscriptionCancelled cancelled, CancellationToken ct) =>
        await bus.PublishAsync(new SubscriptionCancelledIntegrationEvent(cancelled.SubscriptionId, cancelled.CancelledAt,
            await PublishedState.Of(db, cancelled.SubscriptionId, ct)));
}
