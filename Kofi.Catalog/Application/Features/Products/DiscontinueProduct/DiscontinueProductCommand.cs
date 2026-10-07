using Kofi.Common.Abstractions;

namespace Kofi.Catalog.Application.Features.Products.DiscontinueProduct;

public record DiscontinueProductCommand : ICommand
{
    public Guid Id { get; init; }
}
