using Kofi.Application.Persistence;
using Kofi.Common.Domain;
using Kofi.Domain.Products;
using Kofi.Domain.Subscriptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Kofi.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IApplicationDbContext
{
    public DbSet<Product> Products => Set<Product>();

    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    // The shop works in local time, so timestamps are stored without a time
    // zone.
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveColumnType("timestamp without time zone");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // The version a command read goes into the WHERE clause of the update,
        // so the second of two concurrent writes finds no row and fails
        // instead of overwriting the first.
        foreach (var root in modelBuilder.Model.GetEntityTypes()
                     .Where(type => typeof(AggregateRoot).IsAssignableFrom(type.ClrType)))
        {
            modelBuilder.Entity(root.ClrType).HasBaseType((Type?)null);
            modelBuilder.Entity(root.ClrType)
                .Property(nameof(AggregateRoot.Version))
                .IsConcurrencyToken();
        }
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        MoveChangedAggregatesOn();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        MoveChangedAggregatesOn();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    // A command may change only a delivery or an extra, and then only their
    // rows are updated. The root's version has to move as well, or a
    // concurrent write to the same aggregate would not notice.
    private void MoveChangedAggregatesOn()
    {
        ChangeTracker.DetectChanges();

        var roots = ChangeTracker.Entries()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(RootOf)
            .Where(root => root is { State: EntityState.Unchanged or EntityState.Modified })
            .Select(root => root!.Entity)
            .Distinct()
            .ToList();

        foreach (var root in roots)
        {
            var version = Entry(root).Property(nameof(AggregateRoot.Version));
            version.CurrentValue = (int)version.OriginalValue! + 1;
        }
    }

    // An owned part carries its owner's key; follow it up to the root.
    private EntityEntry? RootOf(EntityEntry entry)
    {
        while (entry.Entity is not AggregateRoot)
        {
            var ownership = entry.Metadata.FindOwnership();
            if (ownership is null)
            {
                return null;
            }

            var key = ownership.Properties.Select(p => entry.Property(p.Name).CurrentValue).ToList();
            var owner = ChangeTracker.Entries()
                .FirstOrDefault(candidate =>
                    candidate.Metadata == ownership.PrincipalEntityType
                    && ownership.PrincipalKey.Properties
                        .Select(p => candidate.Property(p.Name).CurrentValue)
                        .SequenceEqual(key));
            if (owner is null)
            {
                return null;
            }

            entry = owner;
        }

        return entry;
    }
}
