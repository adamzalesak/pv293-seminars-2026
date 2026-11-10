using Kofi.Application.Repositories;
using Kofi.Common.Abstractions;

namespace Kofi.Application.Features.Products.ActivateProduct;

public class ActivateProductHandler(IProductRepository products) : ICommandHandler<ActivateProductCommand>
{
    public async Task Handle(ActivateProductCommand command, CancellationToken ct)
    {
        var product = await products.GetById(command.Id) ?? throw new NotFoundException("Product not found");

        product.Activate();
    }
}
