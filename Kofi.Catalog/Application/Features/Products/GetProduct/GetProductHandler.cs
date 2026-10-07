using Kofi.Catalog.Application.Persistence;
using Kofi.Catalog.Domain.Products;
using Kofi.Common.Abstractions;
using Kofi.Warehouse.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Catalog.Application.Features.Products.GetProduct;

public class GetProductHandler(ICatalogDbContext db, IWarehouse stock) : IQueryHandler<GetProductQuery, ProductDetail?>
{
    public async Task<ProductDetail?> Handle(GetProductQuery query, CancellationToken ct)
    {
        var detail = await db.Products.AsNoTracking()
            .Where(p => p.Id == query.Id)
            .Select(p => new ProductDetail
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
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
            .FirstOrDefaultAsync(ct);
        return detail is null ? null : detail with { StockQuantity = await stock.GetAvailable(detail.Id, ct) };
    }
}
