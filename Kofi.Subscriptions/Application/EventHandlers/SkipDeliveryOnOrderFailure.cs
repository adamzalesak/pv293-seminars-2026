using Kofi.Common.Abstractions;
using Kofi.Ordering.Contracts;
using Kofi.Subscriptions.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Subscriptions.Application.EventHandlers;

// Not a compensation: Subscriptions decides what a failed order means for the
// subscription. The delivery is skipped and the next one comes as planned.
public class SkipDeliveryOnOrderFailure(ISubscriptionsDbContext db)
    : IIntegrationEventHandler<OrderFailedIntegrationEvent>, IIntegrationEventHandler<OrderCancelledIntegrationEvent>
{
    public Task Handle(OrderFailedIntegrationEvent failed, CancellationToken ct) =>
        Skip(failed.DeliveryId, null, failed.Reason, ct);

    public Task Handle(OrderCancelledIntegrationEvent cancelled, CancellationToken ct) =>
        cancelled.DeliveryId is { } deliveryId
            ? Skip(deliveryId, cancelled.OrderId, cancelled.Reason, ct)
            : Task.CompletedTask;

    private async Task Skip(Guid deliveryId, Guid? orderId, string reason, CancellationToken ct)
    {
        var subscription = await db.Subscriptions.SingleAsync(s => s.Deliveries.Any(d => d.Id == deliveryId), ct);
        subscription.SkipFailedDelivery(deliveryId, orderId, reason);
    }
}
