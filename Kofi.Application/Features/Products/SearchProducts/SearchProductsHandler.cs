using Kofi.Application.Abstractions;
using Kofi.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Application.Features.Products.SearchProducts;

public class SearchProductsHandler(IApplicationDbContext db) : IQueryHandler<SearchProductsQuery, PagedResult<ProductSummary>>
{
    public async Task<PagedResult<ProductSummary>> Handle(SearchProductsQuery query, CancellationToken ct)
    {
        var q = db.Products.AsNoTracking().AsQueryable();

        if (!query.IncludeInactive)
        {
            q = q.Where(p => p.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            q = q.Where(p =>
                EF.Functions.ILike(p.Name, "%" + query.Search + "%") || EF.Functions.ILike(p.Description, "%" + query.Search + "%"));
        }

        q = query.Sort switch
        {
            "price" => q.OrderBy(p => p.Price.Amount),
            "price_desc" => q.OrderByDescending(p => p.Price.Amount),
            _ => q.OrderBy(p => p.Name)
        };
        var count = await q.CountAsync(ct);
        var rows = await q.Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p => new ProductSummary
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price.Amount,
                StockQuantity = p.StockQuantity,
                IsActive = p.IsActive,
                RoastedOn = p.RoastedOn,
                Origin = p.Origin,
                ImageUrl = p.ImageUrl
            })
            .ToListAsync(ct);
        return new PagedResult<ProductSummary> { Items = rows, Page = query.Page, PageSize = query.PageSize, TotalCount = count };
    }
}
