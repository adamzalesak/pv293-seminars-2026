namespace Kofi.Catalog.Contracts;

public record ProductInfo(Guid Id, string Name, decimal Price, string Currency, bool CanBeSold, bool CanBeSubscribed);
