using Kofi.Catalog.Application.Persistence;
using Kofi.Catalog.Application.ReadModels;
using Kofi.Catalog.Domain.Products;
using Kofi.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Catalog.Infrastructure.Persistence;

public class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : AggregateDbContext(options), ICatalogDbContext
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductSubscription> ProductSubscriptions => Set<ProductSubscription>();

    protected override void ConfigureModel(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
}
