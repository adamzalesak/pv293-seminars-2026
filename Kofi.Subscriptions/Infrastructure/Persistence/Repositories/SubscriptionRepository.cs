using Kofi.Subscriptions.Application.Repositories;
using Kofi.Subscriptions.Domain.Subscriptions;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Subscriptions.Infrastructure.Persistence.Repositories;

public class SubscriptionRepository(SubscriptionsDbContext db) : ISubscriptionRepository
{
    // What the aggregate owns comes with it. Customer and product do not:
    // the subscription only keeps their ids.
    public Task<Subscription?> GetById(Guid id) => db.Subscriptions
        .FirstOrDefaultAsync(s => s.Id == id);

    public void Add(Subscription subscription)
    {
        db.Subscriptions.Add(subscription);
    }
}
