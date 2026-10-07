using Kofi.Catalog.Domain.Products;

namespace Kofi.Catalog.Application.Repositories;

public interface IProductRepository
{
    Task<Product?> GetById(Guid id);
    void Add(Product product);
}
