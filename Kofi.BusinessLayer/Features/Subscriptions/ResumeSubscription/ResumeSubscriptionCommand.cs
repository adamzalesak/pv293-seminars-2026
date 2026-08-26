using Kofi.BusinessLayer.Abstractions;

namespace Kofi.BusinessLayer.Features.Subscriptions.ResumeSubscription;

public record ResumeSubscriptionCommand : ICommand, IForSubscription
{
    public Guid Id { get; init; }
    Guid IForSubscription.SubscriptionId => Id;
    public DateOnly NextDeliveryOn { get; init; }
}
