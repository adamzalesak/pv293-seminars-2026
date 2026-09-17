namespace Kofi.Ordering.Contracts;

public record CustomerInfo(Guid Id, string Name, string Email, bool HasSavedCard, string? CardLabel);
