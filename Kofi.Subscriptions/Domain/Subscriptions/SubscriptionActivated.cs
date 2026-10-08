namespace Kofi.Subscriptions.Domain.Subscriptions;

public record SubscriptionActivated(Guid SubscriptionId, Guid CustomerId, Guid ProductId, DateTime ActivatedAt) : IDomainEvent;
