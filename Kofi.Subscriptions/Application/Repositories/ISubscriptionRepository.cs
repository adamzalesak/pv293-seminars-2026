using Kofi.Subscriptions.Domain.Subscriptions;

namespace Kofi.Subscriptions.Application.Repositories;

public interface ISubscriptionRepository
{
    Task<Subscription?> GetById(Guid id);
    void Add(Subscription subscription);
}
