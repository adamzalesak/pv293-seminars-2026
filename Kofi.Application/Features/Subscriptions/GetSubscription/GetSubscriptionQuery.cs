using Kofi.Application.Abstractions;

namespace Kofi.Application.Features.Subscriptions.GetSubscription;

public record GetSubscriptionQuery : IQuery<SubscriptionDetail?>, IForSubscription
{
    public Guid Id { get; init; }
    Guid IForSubscription.SubscriptionId => Id;
}
