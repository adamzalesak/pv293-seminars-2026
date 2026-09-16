using Kofi.Application.Abstractions;

namespace Kofi.Application.Features.Subscriptions.ResumeSubscription;

public record ResumeSubscriptionCommand : ICommand, IForSubscription
{
    public Guid Id { get; init; }
    Guid IForSubscription.SubscriptionId => Id;
    public DateOnly NextDeliveryOn { get; init; }
}
