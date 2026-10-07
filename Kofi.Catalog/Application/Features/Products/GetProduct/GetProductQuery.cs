using Kofi.Common.Abstractions;

namespace Kofi.Catalog.Application.Features.Products.GetProduct;

public record GetProductQuery : IQuery<ProductDetail?>
{
    public Guid Id { get; init; }
}
