using Kofi.Contracts;

namespace Kofi.Subscriptions.Contracts;

public record SubscriptionProductChangedIntegrationEvent(Guid SubscriptionId, Guid ProductId, SubscriptionState Current)
    : IIntegrationEvent;
