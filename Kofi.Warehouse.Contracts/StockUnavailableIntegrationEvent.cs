using Kofi.Contracts;

namespace Kofi.Warehouse.Contracts;

public record StockUnavailableIntegrationEvent(Guid DeliveryId, string Reason) : IIntegrationEvent;
