namespace Kofi.Ordering.Contracts;

public record CreateOrderRequest(Guid CustomerId, IReadOnlyList<OrderLine> Lines);
