using Kofi.BusinessLayer.Abstractions;

namespace Kofi.BusinessLayer.Features.Subscriptions.GetSubscriptions;

public record GetSubscriptionsQuery : IQuery<List<SubscriptionSummary>>
{
}
