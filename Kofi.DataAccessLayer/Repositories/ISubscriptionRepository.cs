using Kofi.Domain.Subscriptions;

namespace Kofi.DataAccessLayer.Repositories;

public interface ISubscriptionRepository
{
    Task<Subscription?> GetById(Guid id);
    IQueryable<Subscription> Query();
    void Add(Subscription subscription);
}
