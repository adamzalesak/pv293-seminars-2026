using Kofi.Application.Repositories;
using Kofi.Common.Abstractions;
using Kofi.Domain.Products;
using Kofi.Domain.Shared;

namespace Kofi.Application.Features.Products.CreateProduct;

public class CreateProductHandler(IProductRepository products) : ICommandHandler<CreateProductCommand, CreateProductResponse>
{
    public Task<CreateProductResponse> Handle(CreateProductCommand command, CancellationToken ct)
    {
        var product = Product.Create(Guid.NewGuid(), command.Name, command.Description, new Price(command.Price, "EUR"),
            command.StockQuantity, roastedOn: command.RoastedOn, origin: command.Origin, imageUrl: command.ImageUrl);
        products.Add(product);

        return Task.FromResult(new CreateProductResponse { Id = product.Id, Name = product.Name });
    }
}
