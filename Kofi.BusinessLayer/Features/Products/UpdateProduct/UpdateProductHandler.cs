using Kofi.BusinessLayer.Abstractions;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Features.Products.UpdateProduct;

public class UpdateProductHandler(IProductRepository products)
    : ICommandHandler<UpdateProductCommand>
{
    public async Task Handle(UpdateProductCommand command, CancellationToken ct)
    {
        var product = await products.GetById(command.Id)
            ?? throw new DomainException("Product not found");

        product.Name = command.Name;
        product.Description = command.Description;
        product.RoastedOn = command.RoastedOn;
        product.Origin = command.Origin;
        product.ImageUrl = command.ImageUrl;
        products.Update(product);
    }
}
