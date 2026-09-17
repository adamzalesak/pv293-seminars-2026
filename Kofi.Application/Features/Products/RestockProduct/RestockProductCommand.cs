using Kofi.Common.Abstractions;

namespace Kofi.Application.Features.Products.RestockProduct;

public record RestockProductCommand : ICommand
{
    public Guid Id { get; init; }
    public int Quantity { get; init; }
}
