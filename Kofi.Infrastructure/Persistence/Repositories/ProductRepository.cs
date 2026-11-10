using Kofi.Application.Repositories;
using Kofi.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Infrastructure.Persistence.Repositories;

public class ProductRepository(AppDbContext db) : IProductRepository
{
    public Task<Product?> GetById(Guid id) => db.Products.FirstOrDefaultAsync(p => p.Id == id);
    public void Add(Product product) => db.Products.Add(product);
}
