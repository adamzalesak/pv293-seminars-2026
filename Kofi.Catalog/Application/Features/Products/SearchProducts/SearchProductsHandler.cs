using Kofi.Catalog.Application.Persistence;
using Kofi.Catalog.Domain.Products;
using Kofi.Common.Abstractions;
using Kofi.Warehouse.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Catalog.Application.Features.Products.SearchProducts;

public class SearchProductsHandler(ICatalogDbContext db, IWarehouse stock) : IQueryHandler<SearchProductsQuery, PagedResult<ProductSummary>>
{
    public async Task<PagedResult<ProductSummary>> Handle(SearchProductsQuery query, CancellationToken ct)
    {
        var q = db.Products.AsNoTracking().AsQueryable();

        if (!query.IncludeInactive)
        {
            q = q.Where(p => p.Status == ProductStatus.Active || p.Status == ProductStatus.Discontinued);
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
                StockQuantity = 0,
                IsActive = p.Status == ProductStatus.Active || p.Status == ProductStatus.Discontinued,
                Status = p.Status.ToString(),
                CanBeSold = p.Status == ProductStatus.Active || p.Status == ProductStatus.Discontinued,
                CanBeSubscribed = p.Status == ProductStatus.Active,
                RoastedOn = p.RoastedOn,
                Origin = p.Origin,
                ImageUrl = p.ImageUrl
            })
            .ToListAsync(ct);
        var quantities = await stock.GetAvailable(rows.Select(p => p.Id).ToList(), ct);
        rows = rows.Select(p => p with { StockQuantity = quantities.GetValueOrDefault(p.Id) }).ToList();

        return new PagedResult<ProductSummary> { Items = rows, Page = query.Page, PageSize = query.PageSize, TotalCount = count };
    }
}
