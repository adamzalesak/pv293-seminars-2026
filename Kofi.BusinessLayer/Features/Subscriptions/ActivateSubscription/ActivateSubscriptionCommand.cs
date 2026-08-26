using Kofi.BusinessLayer.Abstractions;

namespace Kofi.BusinessLayer.Features.Subscriptions.ActivateSubscription;

public record ActivateSubscriptionCommand : ICommand<ActivateSubscriptionResponse>, IForSubscription
{
    public Guid Id { get; init; }
    // The card the customer just authorised at the provider, or none to
    // charge the card they already saved.
    public Guid? SetupId { get; init; }
    Guid IForSubscription.SubscriptionId => Id;
}
