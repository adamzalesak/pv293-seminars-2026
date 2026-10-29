using Kofi.Warehouse.Application.Persistence;
using Kofi.Warehouse.Contracts;
using Kofi.Warehouse.Domain.Reservations;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Warehouse.Application.Reservations;

// All lines or nothing. The answer is returned and Wolverine sends it in the
// same transaction as the reservation.
public class ReserveStockHandler(IWarehouseDbContext db)
{
    public async Task<object> Handle(ReserveStock command, CancellationToken ct)
    {
        // The same request again gets the same answer.
        if (await db.Reservations.AnyAsync(r => r.Id == command.DeliveryId, ct))
        {
            return new StockReservedIntegrationEvent(command.DeliveryId);
        }

        var lines = command.Lines.GroupBy(l => l.ProductId)
            .Select(g => new ReservedLine(g.Key, new Quantity(g.Sum(l => l.Quantity)).Value))
            .ToList();
        var ids = lines.Select(l => l.ProductId).ToList();
        var products = await db.Products.Where(p => ids.Contains(p.Id)).ToDictionaryAsync(p => p.Id, ct);

        if (lines.Any(l => !products.TryGetValue(l.ProductId, out var product) || product.StockQuantity < l.Quantity))
        {
            return new StockUnavailableIntegrationEvent(command.DeliveryId, "Not enough stock");
        }

        foreach (var line in lines)
        {
            products[line.ProductId].Reserve(new Quantity(line.Quantity));
        }

        db.Reservations.Add(StockReservation.Create(command.DeliveryId, lines));
        return new StockReservedIntegrationEvent(command.DeliveryId);
    }
}
