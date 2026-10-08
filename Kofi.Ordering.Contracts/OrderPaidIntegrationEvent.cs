using Kofi.Contracts;

namespace Kofi.Ordering.Contracts;

public record OrderPaidIntegrationEvent(Guid OrderId, Guid CustomerId, string? CouponCode) : IIntegrationEvent;
