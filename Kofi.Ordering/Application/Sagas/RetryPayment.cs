namespace Kofi.Ordering.Application.Sagas;

// Sent by the saga to itself, a day after a declined payment.
public record RetryPayment(Guid DeliveryId);
