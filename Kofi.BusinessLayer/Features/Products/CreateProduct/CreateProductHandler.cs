using Kofi.BusinessLayer.Abstractions;
using Kofi.DataAccessLayer.Models;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Features.Products.CreateProduct;

public class CreateProductHandler(
    IProductRepository products) : ICommandHandler<CreateProductCommand, CreateProductResponse>
{
    public Task<CreateProductResponse> Handle(CreateProductCommand command, CancellationToken ct)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Description = command.Description,
            Price = command.Price,
            StockQuantity = command.StockQuantity,
            IsActive = true,
            RoastedOn = command.RoastedOn,
            Origin = command.Origin,
            ImageUrl = command.ImageUrl,
        };

        products.Add(product);

        return Task.FromResult(new CreateProductResponse { Id = product.Id, Name = product.Name });
    }
}
