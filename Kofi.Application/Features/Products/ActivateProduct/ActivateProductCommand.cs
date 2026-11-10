using Kofi.Common.Abstractions;

namespace Kofi.Application.Features.Products.ActivateProduct;

public record ActivateProductCommand : ICommand
{
    public Guid Id { get; init; }
}
