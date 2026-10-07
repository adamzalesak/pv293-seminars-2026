using Kofi.Common.Abstractions;

namespace Kofi.Subscriptions.Application.Features.Subscriptions.ResumeSubscription;

public record ResumeSubscriptionCommand : ICommand, IForSubscription
{
    public Guid Id { get; init; }
    Guid IForSubscription.SubscriptionId => Id;
    public DateOnly NextDeliveryOn { get; init; }
}
