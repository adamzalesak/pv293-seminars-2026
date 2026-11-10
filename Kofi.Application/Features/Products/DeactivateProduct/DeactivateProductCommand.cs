using Kofi.Common.Abstractions;

namespace Kofi.Application.Features.Products.DeactivateProduct;

public record DeactivateProductCommand : ICommand
{
    public Guid Id { get; init; }
}
