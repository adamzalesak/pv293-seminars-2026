namespace Kofi.Common.Services;

// What the customer is told when they come back from the payment page.
public enum PaymentOutcome
{
    Paid,
    Pending,
    Declined,
    Cancelled,
}
