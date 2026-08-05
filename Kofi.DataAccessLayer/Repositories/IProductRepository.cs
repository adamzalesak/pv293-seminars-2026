using Kofi.DataAccessLayer.Models;

namespace Kofi.DataAccessLayer.Repositories;

public interface IProductRepository
{
    Task<Product?> GetById(Guid id);
    Task<(List<Product> Items, int TotalCount)> Search(
        string? term, string? sort, int page, int pageSize, bool includeInactive);
    void Add(Product product);
    void Update(Product product);
}
