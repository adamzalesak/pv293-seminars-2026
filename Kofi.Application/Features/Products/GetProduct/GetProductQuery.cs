using Kofi.Application.Abstractions;

namespace Kofi.Application.Features.Products.GetProduct;

public record GetProductQuery : IQuery<ProductDetail?>
{
    public Guid Id { get; init; }
}
