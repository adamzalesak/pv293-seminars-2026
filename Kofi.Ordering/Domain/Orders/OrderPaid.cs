namespace Kofi.Ordering.Domain.Orders;

public record OrderPaid(Guid OrderId, Guid PaymentId) : IDomainEvent;
