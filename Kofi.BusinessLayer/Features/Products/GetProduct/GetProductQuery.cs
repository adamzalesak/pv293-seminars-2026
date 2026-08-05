using Kofi.BusinessLayer.Abstractions;

namespace Kofi.BusinessLayer.Features.Products.GetProduct;

public record GetProductQuery : IQuery<ProductDetail?>
{
    public Guid Id { get; init; }
}
