namespace Kofi.Ordering.Application.Sagas;

// The provider has decided about the payment for an order that fulfils a
// subscription delivery.
public record DeliveryPaymentDecided(Guid DeliveryId, Guid PaymentId, bool Captured);
