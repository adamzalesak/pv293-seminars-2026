using Kofi.Contracts;

namespace Kofi.Ordering.Contracts;

// DeliveryId is set when the order was for a subscription delivery.
public record OrderCancelledIntegrationEvent(Guid OrderId, Guid? DeliveryId, string Reason) : IIntegrationEvent;
