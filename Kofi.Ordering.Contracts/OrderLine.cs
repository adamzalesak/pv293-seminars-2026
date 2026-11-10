namespace Kofi.Ordering.Contracts;

public record OrderLine(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice, string Currency);
