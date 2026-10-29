using Kofi.Catalog.Application.Persistence;
using Kofi.Common.Abstractions;
using Kofi.Subscriptions.Contracts;

namespace Kofi.Catalog.Application.EventHandlers;

public class MoveSubscriptionOnSubscriptionProductChanged(ICatalogDbContext db)
    : IIntegrationEventHandler<SubscriptionProductChangedIntegrationEvent>
{
    public Task Handle(SubscriptionProductChangedIntegrationEvent changed, CancellationToken ct) =>
        ProductSubscriptionCopy.Apply(db, changed.SubscriptionId, changed.Current, ct);
}
