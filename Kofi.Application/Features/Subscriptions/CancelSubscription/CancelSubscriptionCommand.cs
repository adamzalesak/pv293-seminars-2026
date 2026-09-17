using Kofi.Common.Abstractions;

namespace Kofi.Application.Features.Subscriptions.CancelSubscription;

public record CancelSubscriptionCommand : ICommand, IForSubscription
{
    public Guid Id { get; init; }
    Guid IForSubscription.SubscriptionId => Id;
}
