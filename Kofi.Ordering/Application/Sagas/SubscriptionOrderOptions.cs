namespace Kofi.Ordering.Application.Sagas;

// Section "Ordering" in appsettings.
public class SubscriptionOrderOptions
{
    // How long the customer has to update a declined card before the next try.
    public TimeSpan PaymentRetryAfter { get; init; } = TimeSpan.FromDays(1);
}
