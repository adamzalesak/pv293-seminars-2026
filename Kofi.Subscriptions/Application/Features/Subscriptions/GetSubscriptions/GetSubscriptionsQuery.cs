using Kofi.Common.Abstractions;

namespace Kofi.Subscriptions.Application.Features.Subscriptions.GetSubscriptions;

public record GetSubscriptionsQuery : IQuery<List<SubscriptionSummary>>
{
}
