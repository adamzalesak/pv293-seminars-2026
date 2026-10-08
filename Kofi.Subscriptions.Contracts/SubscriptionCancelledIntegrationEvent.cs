using Kofi.Contracts;

namespace Kofi.Subscriptions.Contracts;

public record SubscriptionCancelledIntegrationEvent(Guid SubscriptionId) : IIntegrationEvent;
