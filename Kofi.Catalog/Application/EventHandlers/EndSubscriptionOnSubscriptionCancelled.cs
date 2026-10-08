using Kofi.Catalog.Application.Persistence;
using Kofi.Common.Abstractions;
using Kofi.Subscriptions.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Catalog.Application.EventHandlers;

public class EndSubscriptionOnSubscriptionCancelled(ICatalogDbContext db)
    : IIntegrationEventHandler<SubscriptionCancelledIntegrationEvent>
{
    // A subscription cancelled before it was ever activated is not in the copy.
    public async Task Handle(SubscriptionCancelledIntegrationEvent cancelled, CancellationToken ct)
    {
        if (await db.ProductSubscriptions.FirstOrDefaultAsync(s => s.SubscriptionId == cancelled.SubscriptionId, ct) is { } copy)
        {
            copy.IsActive = false;
        }
    }
}
