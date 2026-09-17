using Kofi.Application.Persistence;
using Kofi.Common.Abstractions;
using Kofi.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Application.Features.Products.GetProduct;

public class GetProductHandler(IApplicationDbContext db) : IQueryHandler<GetProductQuery, ProductDetail?>
{
    public async Task<ProductDetail?> Handle(GetProductQuery query, CancellationToken ct)
    {
        return await db.Products.AsNoTracking()
            .Where(p => p.Id == query.Id)
            .Select(p => new ProductDetail
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price.Amount,
                StockQuantity = p.StockQuantity,
                IsActive = p.Status == ProductStatus.Active || p.Status == ProductStatus.Discontinued,
                Status = p.Status.ToString(),
                CanBeSold = p.Status == ProductStatus.Active || p.Status == ProductStatus.Discontinued,
                CanBeSubscribed = p.Status == ProductStatus.Active,
                RoastedOn = p.RoastedOn,
                Origin = p.Origin,
                ImageUrl = p.ImageUrl
            })
            .FirstOrDefaultAsync(ct);
    }
}
