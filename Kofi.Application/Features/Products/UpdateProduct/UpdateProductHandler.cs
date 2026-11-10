using Kofi.Application.Abstractions;
using Kofi.Application.Repositories;

namespace Kofi.Application.Features.Products.UpdateProduct;

public class UpdateProductHandler(IProductRepository products) : ICommandHandler<UpdateProductCommand>
{
    public async Task Handle(UpdateProductCommand command, CancellationToken ct)
    {
        var product = await products.GetById(command.Id) ?? throw new NotFoundException("Product not found");

        product.UpdateDetails(command.Name, command.Description, command.RoastedOn, command.Origin, command.ImageUrl);
    }
}
