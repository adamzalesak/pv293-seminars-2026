using Kofi.BusinessLayer.Abstractions;

namespace Kofi.BusinessLayer.Features.Products.SearchProducts;

public record SearchProductsQuery : IQuery<PagedResult<ProductSummary>>
{
    public string? Search { get; init; }
    public string? Sort { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public bool IncludeInactive { get; init; }
}
