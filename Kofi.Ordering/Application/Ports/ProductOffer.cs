namespace Kofi.Ordering.Application.Ports;

public record ProductOffer(Guid Id, string Name, decimal Price, string Currency, bool CanBeSold);
