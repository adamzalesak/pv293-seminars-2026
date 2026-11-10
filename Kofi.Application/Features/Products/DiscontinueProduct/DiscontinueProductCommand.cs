using Kofi.Common.Abstractions;

namespace Kofi.Application.Features.Products.DiscontinueProduct;

public record DiscontinueProductCommand : ICommand
{
    public Guid Id { get; init; }
}
