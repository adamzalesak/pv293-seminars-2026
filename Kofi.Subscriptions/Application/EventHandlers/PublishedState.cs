using Kofi.Subscriptions.Application.Persistence;
using Kofi.Subscriptions.Contracts;
using Kofi.Subscriptions.Domain.Subscriptions;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Subscriptions.Application.EventHandlers;

// The domain event carries only its own fact; what other modules need to keep
// a copy current is added here, when the fact leaves the module.
internal static class PublishedState
{
    public static async Task<SubscriptionState> Of(ISubscriptionsDbContext db, Guid subscriptionId, CancellationToken ct)
    {
        var subscription = await db.Subscriptions.AsNoTracking().SingleAsync(s => s.Id == subscriptionId, ct);
        return new SubscriptionState(subscription.Plan.ProductId,
            subscription.Status is SubscriptionStatus.Active or SubscriptionStatus.Paused, subscription.Version);
    }
}
