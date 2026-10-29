using Kofi.Contracts;

namespace Kofi.Warehouse.Contracts;

public record StockReservedIntegrationEvent(Guid DeliveryId) : IIntegrationEvent;
