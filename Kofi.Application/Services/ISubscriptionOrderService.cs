using Kofi.Domain.Subscriptions;

namespace Kofi.Application.Services;

public interface ISubscriptionOrderService
{
    Task Prepare(Subscription subscription, Guid deliveryId, DateTime now, CancellationToken ct);
}
