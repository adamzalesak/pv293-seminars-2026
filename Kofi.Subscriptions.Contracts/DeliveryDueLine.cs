namespace Kofi.Subscriptions.Contracts;

public record DeliveryDueLine(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice, string Currency);
