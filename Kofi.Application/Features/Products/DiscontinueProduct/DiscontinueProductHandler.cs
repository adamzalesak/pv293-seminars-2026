using Kofi.Application.Repositories;
using Kofi.Common.Abstractions;

namespace Kofi.Application.Features.Products.DiscontinueProduct;

public class DiscontinueProductHandler(IProductRepository products) : ICommandHandler<DiscontinueProductCommand>
{
    public async Task Handle(DiscontinueProductCommand command, CancellationToken ct)
    {
        var product = await products.GetById(command.Id) ?? throw new NotFoundException("Product not found");

        product.Discontinue();
    }
}
