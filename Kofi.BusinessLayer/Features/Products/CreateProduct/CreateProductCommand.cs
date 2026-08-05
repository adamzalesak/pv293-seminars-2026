using Kofi.BusinessLayer.Abstractions;

namespace Kofi.BusinessLayer.Features.Products.CreateProduct;

public record CreateProductCommand : ICommand<CreateProductResponse>
{
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public decimal Price { get; init; }
    public int StockQuantity { get; init; }
    public DateOnly? RoastedOn { get; init; }
    public string? Origin { get; init; }
    public string? ImageUrl { get; init; }
}
