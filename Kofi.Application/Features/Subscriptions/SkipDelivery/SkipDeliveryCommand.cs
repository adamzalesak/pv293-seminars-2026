using Kofi.Common.Abstractions;

namespace Kofi.Application.Features.Subscriptions.SkipDelivery;

public record SkipDeliveryCommand : ICommand, IForSubscription
{
    public Guid Id { get; init; }
    Guid IForSubscription.SubscriptionId => Id;
}
