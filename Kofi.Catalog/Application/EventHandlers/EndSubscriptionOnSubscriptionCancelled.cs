using Kofi.Catalog.Application.Persistence;
using Kofi.Common.Abstractions;
using Kofi.Subscriptions.Contracts;

namespace Kofi.Catalog.Application.EventHandlers;

public class EndSubscriptionOnSubscriptionCancelled(ICatalogDbContext db)
    : IIntegrationEventHandler<SubscriptionCancelledIntegrationEvent>
{
    public Task Handle(SubscriptionCancelledIntegrationEvent cancelled, CancellationToken ct) =>
        ProductSubscriptionCopy.Apply(db, cancelled.SubscriptionId, cancelled.Current, ct);
}
