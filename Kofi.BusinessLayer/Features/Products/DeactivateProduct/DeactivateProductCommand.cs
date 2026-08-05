using Kofi.BusinessLayer.Abstractions;

namespace Kofi.BusinessLayer.Features.Products.DeactivateProduct;

public record DeactivateProductCommand : ICommand
{
    public Guid Id { get; init; }
}
