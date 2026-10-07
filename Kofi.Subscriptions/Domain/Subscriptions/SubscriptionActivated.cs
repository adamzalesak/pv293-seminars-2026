namespace Kofi.Subscriptions.Domain.Subscriptions;

public record SubscriptionActivated(Guid SubscriptionId) : IDomainEvent;
