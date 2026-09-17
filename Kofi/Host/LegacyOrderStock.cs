using Kofi.Application.Repositories;
using Kofi.Common.Abstractions;
using Kofi.Domain.Products;
using Kofi.Domain.Shared;
using Kofi.Ordering.Application.Ports;

namespace Kofi.Host;

// The remaining shared product model implements the new module's port.
public class LegacyOrderStock(IProductRepository products) : IOrderStock
{
    private async Task<List<(Product Product, int Quantity)>> Load(IReadOnlyList<StockLine> lines)
    {
        var rows = new List<(Product, int)>();

        foreach (var line in lines.GroupBy(l => l.ProductId))
        {
            var product = await products.GetById(line.Key) ?? throw new NotFoundException("Product not found");

            var quantity = line.Sum(l => l.Quantity);
            product.CheckAvailable(new Quantity(quantity));
            rows.Add((product, quantity));
        }

        return rows;
    }

    public async Task CheckAvailable(IReadOnlyList<StockLine> lines, CancellationToken ct = default) => _ = await Load(lines);

    public async Task Reserve(IReadOnlyList<StockLine> lines, CancellationToken ct = default)
    {
        foreach (var (product, quantity) in await Load(lines))
        {
            product.Reserve(new Quantity(quantity));
        }
    }

    public async Task Release(Guid productId, int quantity, CancellationToken ct = default)
    {
        var product = await products.GetById(productId) ?? throw new NotFoundException("Product not found");

        product.Release(new Quantity(quantity));
    }
}
