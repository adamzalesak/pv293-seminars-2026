namespace Kofi.Subscriptions.Domain.Subscriptions;

// A delivery is due, with the lines and prices agreed for it.
public record DeliveryDue(Guid SubscriptionId, Guid DeliveryId, Guid CustomerId, IReadOnlyList<DeliveryLine> Lines, DateTime DueAt)
    : IDomainEvent;
