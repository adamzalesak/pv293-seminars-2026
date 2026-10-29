using Kofi.Warehouse.Application.Persistence;
using Kofi.Warehouse.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Warehouse.Application.Reservations;

// The compensating action for ReserveStock: a new step that puts the goods
// back, not an undo of the old one.
public class ReleaseStockHandler(IWarehouseDbContext db)
{
    public async Task<StockReleasedIntegrationEvent> Handle(ReleaseStock command, CancellationToken ct)
    {
        var reservation = await db.Reservations.FirstOrDefaultAsync(r => r.Id == command.DeliveryId, ct);

        foreach (var line in reservation?.Release() ?? [])
        {
            var product = await db.Products.SingleAsync(p => p.Id == line.ProductId, ct);
            product.Release(new Quantity(line.Quantity));
        }

        return new StockReleasedIntegrationEvent(command.DeliveryId);
    }
}
