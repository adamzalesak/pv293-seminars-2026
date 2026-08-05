using Kofi.BusinessLayer.Abstractions;

namespace Kofi.BusinessLayer.Features.Products.ChangeProductPrice;

public record ChangeProductPriceCommand : ICommand
{
    public Guid Id { get; init; }
    public decimal Price { get; init; }
}
