using Kofi.Application.Abstractions;
using Kofi.Application.Repositories;
using Kofi.Domain.Shared;

namespace Kofi.Application.Features.Products.ChangeProductPrice;

public class ChangeProductPriceHandler(IProductRepository products) : ICommandHandler<ChangeProductPriceCommand>
{
    public async Task Handle(ChangeProductPriceCommand command, CancellationToken ct)
    {
        var product = await products.GetById(command.Id) ?? throw new NotFoundException("Product not found");

        product.ChangePrice(new Price(command.Price, "EUR"));
    }
}
