using Kofi.Warehouse.Domain.Products;
using Kofi.Warehouse.Domain.Reservations;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Warehouse.Application.Persistence;

public interface IWarehouseDbContext
{
    DbSet<Product> Products { get; }
    DbSet<StockReservation> Reservations { get; }
}
