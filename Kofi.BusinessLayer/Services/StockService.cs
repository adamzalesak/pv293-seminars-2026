using Kofi.DataAccessLayer.Models;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Services;

public class StockService(ProductRepository products)
{
    public async Task<bool> HasEnough(Guid productId, int quantity)
    {
        var product = await products.GetById(productId);
        if (product == null)
        {
            return false;
        }
        return product.StockQuantity >= quantity;
    }

    public async Task Reserve(Product product, int quantity)
    {
        product.StockQuantity = product.StockQuantity - quantity;
        await products.Update(product);
    }

    public async Task Release(Guid productId, int quantity)
    {
        var product = await products.GetById(productId);
        if (product != null)
        {
            product.StockQuantity = product.StockQuantity + quantity;
            await products.Update(product);
        }
    }
}
