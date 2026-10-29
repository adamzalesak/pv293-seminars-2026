using Kofi.Contracts;

namespace Kofi.Subscriptions.Contracts;

// A subscription delivery is due, with the lines and prices agreed for it.
// Subscriptions does not know who turns it into an order; it learns the
// outcome from Ordering's events.
public record DeliveryDueIntegrationEvent(
    Guid DeliveryId, Guid SubscriptionId, Guid CustomerId, IReadOnlyList<DeliveryDueLine> Lines, DateTime DueAt)
    : IIntegrationEvent;
