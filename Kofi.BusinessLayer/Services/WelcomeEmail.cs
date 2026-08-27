namespace Kofi.BusinessLayer.Services;

// What the welcome e-mail says, read for it alone.
public record WelcomeEmail(Guid SubscriptionId, string CustomerEmail, string ProductName, int Quantity, int IntervalDays);
