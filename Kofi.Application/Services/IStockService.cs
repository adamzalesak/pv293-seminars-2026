using Kofi.Domain.Products;

namespace Kofi.Application.Services;

public interface IStockService
{
    void Reserve(Product product, int quantity);
    Task Release(Guid productId, int quantity);
}
