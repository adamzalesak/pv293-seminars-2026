namespace Kofi.Subscriptions.Application.Features.Subscriptions.ResumeSubscription;

public record ResumeSubscriptionRequest
{
    public DateOnly NextDeliveryOn { get; init; }
}
