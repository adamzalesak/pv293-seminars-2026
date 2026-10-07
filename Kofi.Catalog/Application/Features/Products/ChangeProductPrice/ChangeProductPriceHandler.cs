using Kofi.Catalog.Application.Repositories;
using Kofi.Common.Abstractions;

namespace Kofi.Catalog.Application.Features.Products.ChangeProductPrice;

public class ChangeProductPriceHandler(IProductRepository products) : ICommandHandler<ChangeProductPriceCommand>
{
    public async Task Handle(ChangeProductPriceCommand command, CancellationToken ct)
    {
        var product = await products.GetById(command.Id) ?? throw new NotFoundException("Product not found");

        product.ChangePrice(new Price(command.Price, "EUR"));
    }
}
