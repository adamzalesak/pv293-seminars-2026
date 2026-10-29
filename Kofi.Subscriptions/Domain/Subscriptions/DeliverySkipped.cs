namespace Kofi.Subscriptions.Domain.Subscriptions;

// No order came of a delivery; the customer is told why.
public record DeliverySkipped(Guid SubscriptionId, Guid DeliveryId, string Reason) : IDomainEvent;
