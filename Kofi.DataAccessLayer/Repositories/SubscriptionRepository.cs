using Microsoft.EntityFrameworkCore;
using Kofi.DataAccessLayer.Data;
using Kofi.DataAccessLayer.Models;

namespace Kofi.DataAccessLayer.Repositories;

public class SubscriptionRepository(AppDbContext db) : ISubscriptionRepository
{
    public Task<Subscription?> GetById(Guid id) => db.Subscriptions
        .Include(s => s.Customer)
        .Include(s => s.Product)
        .Include(s => s.Deliveries).ThenInclude(d => d.Order)
        .Include(s => s.Deliveries).ThenInclude(d => d.Extras)
        .FirstOrDefaultAsync(s => s.Id == id);

    public IQueryable<Subscription> Query() => db.Subscriptions;

    public void Add(Subscription subscription)
    {
        db.Subscriptions.Add(subscription);
    }
}
