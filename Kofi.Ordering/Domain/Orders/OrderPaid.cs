namespace Kofi.Ordering.Domain.Orders;

public record OrderPaid(Guid OrderId, Guid CustomerId, string? CouponCode) : IDomainEvent;
