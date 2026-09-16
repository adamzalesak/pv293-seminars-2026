namespace Kofi.Application.Services;

// What the payment provider says about a customer's permission to charge a
// card later.
public enum CardSetupStatus
{
    Created,
    Succeeded,
    Declined,
}
