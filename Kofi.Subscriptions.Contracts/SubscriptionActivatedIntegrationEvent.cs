using Kofi.Contracts;

namespace Kofi.Subscriptions.Contracts;

public record SubscriptionActivatedIntegrationEvent(Guid SubscriptionId, Guid CustomerId, Guid ProductId, DateTime ActivatedAt)
    : IIntegrationEvent;
