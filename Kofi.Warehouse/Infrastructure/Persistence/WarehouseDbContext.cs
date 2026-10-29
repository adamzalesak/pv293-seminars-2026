using Kofi.Common.Persistence;
using Kofi.Warehouse.Application.Persistence;
using Kofi.Warehouse.Domain.Products;
using Kofi.Warehouse.Domain.Reservations;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Warehouse.Infrastructure.Persistence;

public class WarehouseDbContext(DbContextOptions<WarehouseDbContext> options) : AggregateDbContext(options), IWarehouseDbContext
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockReservation> Reservations => Set<StockReservation>();

    protected override void ConfigureModel(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WarehouseDbContext).Assembly);
}
