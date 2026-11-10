using Kofi.Application.Persistence;
using Kofi.Domain.Products;
using Kofi.Ordering.Application.Ports;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Host;

public class LegacyProductCatalog(IApplicationDbContext db) : IProductCatalog
{
    public Task<ProductOffer?> GetProduct(Guid id, CancellationToken ct = default) => db.Products.AsNoTracking()
        .Where(p => p.Id == id)
        .Select(p => new ProductOffer(p.Id, p.Name, p.Price.Amount, p.Price.Currency,
            p.Status == ProductStatus.Active || p.Status == ProductStatus.Discontinued))
        .FirstOrDefaultAsync(ct);
}
