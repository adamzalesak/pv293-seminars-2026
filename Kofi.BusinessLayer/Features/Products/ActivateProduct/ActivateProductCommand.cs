using Kofi.BusinessLayer.Abstractions;

namespace Kofi.BusinessLayer.Features.Products.ActivateProduct;

public record ActivateProductCommand : ICommand
{
    public Guid Id { get; init; }
}
