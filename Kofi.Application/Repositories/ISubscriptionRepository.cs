using Kofi.Domain.Subscriptions;

namespace Kofi.Application.Repositories;

public interface ISubscriptionRepository
{
    Task<Subscription?> GetById(Guid id);
    void Add(Subscription subscription);
}
