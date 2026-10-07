using Kofi.Catalog.Application.Repositories;
using Kofi.Catalog.Domain.Products;
using Kofi.Common.Abstractions;
using Kofi.Warehouse.Contracts;

namespace Kofi.Catalog.Application.Features.Products.CreateProduct;

public class CreateProductHandler(IProductRepository products, IWarehouse stock)
    : ICommandHandler<CreateProductCommand, CreateProductResponse>
{
    public async Task<CreateProductResponse> Handle(CreateProductCommand command, CancellationToken ct)
    {
        var product = Product.Create(Guid.NewGuid(), command.Name, command.Description, new Price(command.Price, "EUR"),
            roastedOn: command.RoastedOn, origin: command.Origin, imageUrl: command.ImageUrl);
        products.Add(product);
        await stock.AddProduct(product.Id, command.StockQuantity, ct: ct);

        return new CreateProductResponse { Id = product.Id, Name = product.Name };
    }
}
