using Kofi.Application.Abstractions;

namespace Kofi.Application.Features.Products.DeactivateProduct;

public record DeactivateProductCommand : ICommand
{
    public Guid Id { get; init; }
}
