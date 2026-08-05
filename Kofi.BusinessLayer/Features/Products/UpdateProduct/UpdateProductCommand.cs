using Kofi.BusinessLayer.Abstractions;

namespace Kofi.BusinessLayer.Features.Products.UpdateProduct;

public record UpdateProductCommand : ICommand
{
    public Guid Id { get; init; }
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public DateOnly? RoastedOn { get; init; }
    public string? Origin { get; init; }
    public string? ImageUrl { get; init; }
}
