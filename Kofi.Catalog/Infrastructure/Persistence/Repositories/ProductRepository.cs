using Kofi.Catalog.Application.Repositories;
using Kofi.Catalog.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Catalog.Infrastructure.Persistence.Repositories;

public class ProductRepository(CatalogDbContext db) : IProductRepository
{
    public Task<Product?> GetById(Guid id) => db.Products.FirstOrDefaultAsync(p => p.Id == id);
    public void Add(Product product) => db.Products.Add(product);
}
