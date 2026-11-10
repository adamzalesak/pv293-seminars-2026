using Kofi.Application.Abstractions;

namespace Kofi.Application.Features.Products.ChangeProductPrice;

public record ChangeProductPriceCommand : ICommand
{
    public Guid Id { get; init; }
    public decimal Price { get; init; }
}
