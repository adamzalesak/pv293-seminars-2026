using Kofi.Common.Abstractions;

namespace Kofi.Application.Features.Subscriptions.GetSubscriptions;

public record GetSubscriptionsQuery : IQuery<List<SubscriptionSummary>>
{
}
