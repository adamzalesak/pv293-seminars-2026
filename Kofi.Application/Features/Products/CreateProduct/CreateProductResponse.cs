namespace Kofi.Application.Features.Products.CreateProduct;

public record CreateProductResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
}
