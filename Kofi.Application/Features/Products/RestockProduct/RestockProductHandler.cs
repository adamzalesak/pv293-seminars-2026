using Kofi.Application.Repositories;
using Kofi.Common.Abstractions;
using Kofi.Domain.Shared;

namespace Kofi.Application.Features.Products.RestockProduct;

public class RestockProductHandler(IProductRepository products) : ICommandHandler<RestockProductCommand>
{
    public async Task Handle(RestockProductCommand command, CancellationToken ct)
    {
        var product = await products.GetById(command.Id) ?? throw new NotFoundException("Product not found");

        product.Restock(new Quantity(command.Quantity));
    }
}
