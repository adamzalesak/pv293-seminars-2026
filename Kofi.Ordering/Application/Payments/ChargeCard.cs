namespace Kofi.Ordering.Application.Payments;

// Charge a saved card for an order. The idempotency key names the attempt, so
// sending the same message again asks the provider for the same payment.
public record ChargeCard(Guid OrderId, decimal Amount, string Currency, string PaymentMethodToken, string IdempotencyKey);
