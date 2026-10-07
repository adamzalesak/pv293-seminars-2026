using Kofi.Subscriptions.Domain.Subscriptions;

namespace Kofi.Subscriptions.Application.Services;

public interface ISubscriptionOrderService
{
    Task Prepare(Subscription subscription, Guid deliveryId, DateTime now, CancellationToken ct);
}
