using Kofi.Application.Repositories;
using Kofi.Domain.Products;

namespace Kofi.Application.Services;

public class StockService(IProductRepository products) : IStockService
{
    public void Reserve(Product product, int quantity)
    {
        product.Reserve(new Kofi.Domain.Shared.Quantity(quantity));
    }

    public async Task Release(Guid productId, int quantity)
    {
        var product = await products.GetById(productId);
        if (product != null)
        {
            product.Release(new Kofi.Domain.Shared.Quantity(quantity));
        }
    }
}
