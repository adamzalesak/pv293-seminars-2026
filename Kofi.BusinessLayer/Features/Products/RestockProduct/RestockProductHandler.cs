using Kofi.BusinessLayer.Abstractions;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Features.Products.RestockProduct;

public class RestockProductHandler(IProductRepository products)
    : ICommandHandler<RestockProductCommand>
{
    public async Task Handle(RestockProductCommand command, CancellationToken ct)
    {
        var product = await products.GetById(command.Id)
            ?? throw new DomainException("Product not found");

        product.StockQuantity = product.StockQuantity + command.Quantity;
        products.Update(product);
    }
}
