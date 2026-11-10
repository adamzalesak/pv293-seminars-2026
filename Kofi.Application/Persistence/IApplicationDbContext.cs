using Kofi.Domain.Products;
using Kofi.Domain.Subscriptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Kofi.Application.Persistence;

public interface IApplicationDbContext
{
    DbSet<Product> Products { get; }
    DbSet<Subscription> Subscriptions { get; }
    ChangeTracker ChangeTracker { get; }
    DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
