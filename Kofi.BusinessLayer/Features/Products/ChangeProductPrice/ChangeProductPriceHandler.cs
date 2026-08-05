using Kofi.BusinessLayer.Abstractions;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Features.Products.ChangeProductPrice;

public class ChangeProductPriceHandler(IProductRepository products)
    : ICommandHandler<ChangeProductPriceCommand>
{
    public async Task Handle(ChangeProductPriceCommand command, CancellationToken ct)
    {
        var product = await products.GetById(command.Id)
            ?? throw new DomainException("Product not found");

        product.Price = command.Price;
        products.Update(product);
    }
}
