using Kofi.BusinessLayer.Abstractions;

namespace Kofi.BusinessLayer.Features.Products.RestockProduct;

public record RestockProductCommand : ICommand
{
    public Guid Id { get; init; }
    public int Quantity { get; init; }
}
