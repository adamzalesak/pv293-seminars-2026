using Kofi.Common.Persistence;
using Kofi.Subscriptions.Application.Persistence;
using Kofi.Subscriptions.Domain.Subscriptions;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Subscriptions.Infrastructure.Persistence;

public class SubscriptionsDbContext(DbContextOptions<SubscriptionsDbContext> options) : AggregateDbContext(options), ISubscriptionsDbContext
{
    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    protected override void ConfigureModel(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SubscriptionsDbContext).Assembly);
}
