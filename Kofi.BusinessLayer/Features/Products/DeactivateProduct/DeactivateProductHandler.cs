using Kofi.BusinessLayer.Abstractions;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Features.Products.DeactivateProduct;

public class DeactivateProductHandler(IProductRepository products)
    : ICommandHandler<DeactivateProductCommand>
{
    public async Task Handle(DeactivateProductCommand command, CancellationToken ct)
    {
        var product = await products.GetById(command.Id)
            ?? throw new DomainException("Product not found");

        product.IsActive = false;
        products.Update(product);
    }
}
