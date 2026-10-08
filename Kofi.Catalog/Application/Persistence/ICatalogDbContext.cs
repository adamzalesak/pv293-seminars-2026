using Kofi.Catalog.Application.ReadModels;
using Kofi.Catalog.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Catalog.Application.Persistence;

public interface ICatalogDbContext
{
    DbSet<Product> Products { get; }
    DbSet<ProductSubscription> ProductSubscriptions { get; }
}
