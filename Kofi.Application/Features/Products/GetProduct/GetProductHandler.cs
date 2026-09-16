using Kofi.Application.Abstractions;
using Kofi.Application.Persistence;
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
                IsActive = p.IsActive,
                RoastedOn = p.RoastedOn,
                Origin = p.Origin,
                ImageUrl = p.ImageUrl
            })
            .FirstOrDefaultAsync(ct);
    }
}
