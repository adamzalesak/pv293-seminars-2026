namespace Kofi.Ordering.Application.Sagas;

public record SubscriptionOrderLine(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice, string Currency);
