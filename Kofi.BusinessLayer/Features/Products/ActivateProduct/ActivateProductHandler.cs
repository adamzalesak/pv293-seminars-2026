using Kofi.BusinessLayer.Abstractions;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Features.Products.ActivateProduct;

public class ActivateProductHandler(IProductRepository products)
    : ICommandHandler<ActivateProductCommand>
{
    public async Task Handle(ActivateProductCommand command, CancellationToken ct)
    {
        var product = await products.GetById(command.Id)
            ?? throw new DomainException("Product not found");

        product.IsActive = true;
        products.Update(product);
    }
}
