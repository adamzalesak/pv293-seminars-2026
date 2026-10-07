using Kofi.Common.Abstractions;

namespace Kofi.Catalog.Application.Features.Products.DeactivateProduct;

public record DeactivateProductCommand : ICommand
{
    public Guid Id { get; init; }
}
