namespace Kofi.Ordering.Application.Ports;

public record StockLine(Guid ProductId, int Quantity);
