using Kofi.Common.Abstractions;
using Kofi.Warehouse.Application.Persistence;
using Kofi.Warehouse.Contracts;
using Kofi.Warehouse.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Warehouse.Application;

public class WarehouseModule(IWarehouseDbContext db) : IWarehouse
{
    public async Task AddProduct(Guid productId, int quantity, string shelfLocation = "A-01", CancellationToken ct = default)
    {
        if (await db.Products.AnyAsync(p => p.Id == productId, ct))
        {
            throw new DomainRuleException("Stock item already exists.");
        }

        db.Products.Add(Product.Create(productId, quantity, shelfLocation));
    }

    private async Task<List<(Product Product, int Quantity)>> Load(IReadOnlyList<StockLine> lines, CancellationToken ct)
    {
        var result = new List<(Product, int)>();

        foreach (var line in lines.GroupBy(l => l.ProductId))
        {
            var product = await db.Products.FirstOrDefaultAsync(p => p.Id == line.Key, ct) ??
                          throw new NotFoundException("Product stock not found");

            var quantity = line.Sum(l => l.Quantity);
            _ = new Quantity(quantity);

            if (product.StockQuantity < quantity)
            {
                throw new DomainRuleException("Not enough stock.");
            }

            result.Add((product, quantity));
        }

        return result;
    }

    public async Task CheckAvailable(IReadOnlyList<StockLine> lines, CancellationToken ct = default) => _ = await Load(lines, ct);

    public async Task Reserve(IReadOnlyList<StockLine> lines, CancellationToken ct = default)
    {
        foreach (var (product, quantity) in await Load(lines, ct))
        {
            product.Reserve(new Quantity(quantity));
        }
    }

    public async Task Release(Guid productId, int quantity, CancellationToken ct = default)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == productId, ct) ??
                      throw new NotFoundException("Product stock not found");

        product.Release(new Quantity(quantity));
    }

    public async Task<int> GetAvailable(Guid productId, CancellationToken ct = default) =>
        await db.Products.Where(p => p.Id == productId).Select(p => p.StockQuantity).SingleOrDefaultAsync(ct);

    public async Task<IReadOnlyDictionary<Guid, int>> GetAvailable(IReadOnlyList<Guid> ids, CancellationToken ct = default) =>
        await db.Products.Where(p => ids.Contains(p.Id)).ToDictionaryAsync(p => p.Id, p => p.StockQuantity, ct);
}
