using Kofi.DataAccessLayer.Models;

namespace Kofi.BusinessLayer.Services;

public interface IStockService
{
    Task Reserve(Product product, int quantity);
    Task Release(Guid productId, int quantity);
}
