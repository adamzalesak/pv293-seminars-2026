namespace Kofi.Subscriptions.Domain.Subscriptions;

public record SubscriptionProductChanged(Guid SubscriptionId, Guid ProductId) : IDomainEvent;
