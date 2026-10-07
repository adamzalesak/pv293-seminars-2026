using Kofi.Subscriptions.Domain.Subscriptions;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Subscriptions.Application.Persistence;

public interface ISubscriptionsDbContext
{
    DbSet<Subscription> Subscriptions { get; }
}
