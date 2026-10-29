namespace Kofi.Warehouse.Contracts;

// Give back what was reserved for a delivery. Asking twice gives back once;
// the answer is StockReleasedIntegrationEvent.
public record ReleaseStock(Guid DeliveryId);
