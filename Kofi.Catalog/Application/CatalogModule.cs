using Kofi.Catalog.Application.Persistence;
using Kofi.Catalog.Contracts;
using Kofi.Catalog.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Catalog.Application;

public class CatalogModule(ICatalogDbContext db) : ICatalog
{
    public Task<ProductInfo?> GetProduct(Guid id, CancellationToken ct = default) => db.Products.AsNoTracking()
        .Where(p => p.Id == id)
        .Select(p => new ProductInfo(p.Id, p.Name, p.Price.Amount, p.Price.Currency,
            p.Status == ProductStatus.Active || p.Status == ProductStatus.Discontinued, p.Status == ProductStatus.Active))
        .FirstOrDefaultAsync(ct);
}
