using Kofi.Common.Persistence;
using Kofi.Warehouse.Application.Persistence;
using Kofi.Warehouse.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Warehouse.Infrastructure.Persistence;

public class WarehouseDbContext(DbContextOptions<WarehouseDbContext> options) : AggregateDbContext(options), IWarehouseDbContext
{
    public DbSet<Product> Products => Set<Product>();

    protected override void ConfigureModel(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WarehouseDbContext).Assembly);
}
