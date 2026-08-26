namespace Kofi.Domain.Subscriptions;

public record SubscriptionActivated(Guid SubscriptionId) : IDomainEvent;
