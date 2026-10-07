using Kofi.Common.Abstractions;

namespace Kofi.Catalog.Application.Features.Products.ActivateProduct;

public record ActivateProductCommand : ICommand
{
    public Guid Id { get; init; }
}
