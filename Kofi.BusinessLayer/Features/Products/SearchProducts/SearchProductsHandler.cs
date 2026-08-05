using Kofi.BusinessLayer.Abstractions;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Features.Products.SearchProducts;

public class SearchProductsHandler(IProductRepository products)
    : IQueryHandler<SearchProductsQuery, PagedResult<ProductSummary>>
{
    public async Task<PagedResult<ProductSummary>> Handle(SearchProductsQuery query, CancellationToken ct)
    {
        var result = await products.Search(query.Search, query.Sort, query.Page, query.PageSize, query.IncludeInactive);
        var items = result.Items.Select(p => new ProductSummary
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            StockQuantity = p.StockQuantity,
            IsActive = p.IsActive,
            RoastedOn = p.RoastedOn,
            Origin = p.Origin,
            ImageUrl = p.ImageUrl,
        }).ToList();
        return new PagedResult<ProductSummary>
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = result.TotalCount,
        };
    }
}
