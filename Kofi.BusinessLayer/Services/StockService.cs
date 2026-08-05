using Kofi.DataAccessLayer.Models;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Services;

public class StockService(IProductRepository products) : IStockService
{
    public void Reserve(Product product, int quantity)
    {
        product.StockQuantity = product.StockQuantity - quantity;
        products.Update(product);
    }

    public async Task Release(Guid productId, int quantity)
    {
        var product = await products.GetById(productId);
        if (product != null)
        {
            product.StockQuantity = product.StockQuantity + quantity;
            products.Update(product);
        }
    }
}
