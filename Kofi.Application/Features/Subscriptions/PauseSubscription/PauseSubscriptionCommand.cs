using Kofi.Common.Abstractions;

namespace Kofi.Application.Features.Subscriptions.PauseSubscription;

public record PauseSubscriptionCommand : ICommand, IForSubscription
{
    public Guid Id { get; init; }
    Guid IForSubscription.SubscriptionId => Id;
}
