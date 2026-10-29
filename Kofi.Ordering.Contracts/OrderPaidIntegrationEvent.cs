using Kofi.Contracts;

namespace Kofi.Ordering.Contracts;

// DeliveryId is set when the order fulfils a subscription delivery.
public record OrderPaidIntegrationEvent(Guid OrderId, Guid CustomerId, string? CouponCode, Guid? DeliveryId) : IIntegrationEvent;
