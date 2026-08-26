namespace Kofi.BusinessLayer.Services;

public record SubscriptionOrderLine(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice, string Currency);
