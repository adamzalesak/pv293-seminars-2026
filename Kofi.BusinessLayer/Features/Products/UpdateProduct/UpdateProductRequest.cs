namespace Kofi.BusinessLayer.Features.Products.UpdateProduct;

// Price and stock have their own operations.
public record UpdateProductRequest
{
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public DateOnly? RoastedOn { get; init; }
    public string? Origin { get; init; }
    public string? ImageUrl { get; init; }
}
