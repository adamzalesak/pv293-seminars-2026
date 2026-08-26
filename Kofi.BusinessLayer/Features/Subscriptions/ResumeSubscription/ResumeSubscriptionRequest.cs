namespace Kofi.BusinessLayer.Features.Subscriptions.ResumeSubscription;

public record ResumeSubscriptionRequest
{
    public DateOnly NextDeliveryOn { get; init; }
}
