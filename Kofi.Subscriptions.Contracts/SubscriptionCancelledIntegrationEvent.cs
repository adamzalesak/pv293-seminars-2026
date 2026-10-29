using Kofi.Contracts;

namespace Kofi.Subscriptions.Contracts;

public record SubscriptionCancelledIntegrationEvent(Guid SubscriptionId, DateTime CancelledAt, SubscriptionState Current)
    : IIntegrationEvent;
