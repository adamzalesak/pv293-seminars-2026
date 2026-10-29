using Kofi.Contracts;

namespace Kofi.Warehouse.Contracts;

public record StockReleasedIntegrationEvent(Guid DeliveryId) : IIntegrationEvent;
