using Kofi.Domain.Products;

namespace Kofi.Application.Repositories;

public interface IProductRepository
{
    Task<Product?> GetById(Guid id);
    void Add(Product product);
}
