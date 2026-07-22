namespace Kofi.BusinessLayer.Services;

// What the payment provider says about one payment.
public enum PaymentStatus
{
    Created,
    Captured,
    Declined,
    Cancelled,
    Refunded,
}
