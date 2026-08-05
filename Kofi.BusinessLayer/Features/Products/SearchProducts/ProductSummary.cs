namespace Kofi.BusinessLayer.Features.Products.SearchProducts;

public record ProductSummary
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required decimal Price { get; init; }
    public required int StockQuantity { get; init; }
    public required bool IsActive { get; init; }
    public DateOnly? RoastedOn { get; init; }
    public string? Origin { get; init; }
    public string? ImageUrl { get; init; }
}
