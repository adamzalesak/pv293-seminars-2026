using Kofi.Catalog.Application.Repositories;
using Kofi.Common.Abstractions;

namespace Kofi.Catalog.Application.Features.Products.DeactivateProduct;

public class DeactivateProductHandler(IProductRepository products) : ICommandHandler<DeactivateProductCommand>
{
    public async Task Handle(DeactivateProductCommand command, CancellationToken ct)
    {
        var product = await products.GetById(command.Id) ?? throw new NotFoundException("Product not found");

        product.Deactivate();
    }
}
