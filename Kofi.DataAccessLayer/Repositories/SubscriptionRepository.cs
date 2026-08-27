using Microsoft.EntityFrameworkCore;
using Kofi.DataAccessLayer.Data;
using Kofi.Domain.Subscriptions;

namespace Kofi.DataAccessLayer.Repositories;

public class SubscriptionRepository(AppDbContext db) : ISubscriptionRepository
{
    // What the aggregate owns comes with it. Customer and product do not:
    // the subscription only keeps their ids.
    public Task<Subscription?> GetById(Guid id) => db.Subscriptions
        .FirstOrDefaultAsync(s => s.Id == id);

    public IQueryable<Subscription> Query() => db.Subscriptions;

    public void Add(Subscription subscription)
    {
        db.Subscriptions.Add(subscription);
    }
}
