namespace Kofi.Warehouse.Contracts;

// Ordering asks for the goods of one delivery; the answer is
// StockReservedIntegrationEvent or StockUnavailableIntegrationEvent.
public record ReserveStock(Guid DeliveryId, IReadOnlyList<StockLine> Lines);
