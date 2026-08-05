using Kofi.DataAccessLayer.Models;

namespace Kofi.BusinessLayer.Services;

public interface IStockService
{
    void Reserve(Product product, int quantity);
    Task Release(Guid productId, int quantity);
}
