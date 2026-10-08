namespace Kofi.Subscriptions.Domain.Subscriptions;

public record SubscriptionCancelled(Guid SubscriptionId) : IDomainEvent;
