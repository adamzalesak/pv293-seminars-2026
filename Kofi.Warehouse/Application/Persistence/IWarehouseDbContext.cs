using Kofi.Warehouse.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Warehouse.Application.Persistence;

public interface IWarehouseDbContext
{
    DbSet<Product> Products { get; }
}
