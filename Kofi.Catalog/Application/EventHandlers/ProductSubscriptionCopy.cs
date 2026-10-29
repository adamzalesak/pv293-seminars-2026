using Kofi.Catalog.Application.Persistence;
using Kofi.Catalog.Application.ReadModels;
using Kofi.Subscriptions.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Catalog.Application.EventHandlers;

// Every subscription event carries the subscription's whole state and its
// version, so any of them can bring the copy up to date, and one that arrives
// late, behind a newer one, is ignored. Each handler runs on its own queue and
// nothing keeps them in order: Wolverine's partitioned processing would, but
// in this version it loops with more than one handler per event.
internal static class ProductSubscriptionCopy
{
    public static async Task Apply(ICatalogDbContext db, Guid subscriptionId, SubscriptionState state, CancellationToken ct)
    {
        var copy = await db.ProductSubscriptions.FirstOrDefaultAsync(s => s.SubscriptionId == subscriptionId, ct);
        if (copy is null)
        {
            copy = new ProductSubscription { SubscriptionId = subscriptionId };
            db.ProductSubscriptions.Add(copy);
        }
        else if (copy.Version >= state.Version)
        {
            return;
        }

        copy.ProductId = state.ProductId;
        copy.IsActive = state.IsRunning;
        copy.Version = state.Version;
    }
}
