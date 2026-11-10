namespace Kofi.Application.Features.Subscriptions;

// A request about one subscription. SubscriptionOwnershipBehavior makes sure
// a customer only ever reaches their own.
public interface IForSubscription
{
    Guid SubscriptionId { get; }
}
