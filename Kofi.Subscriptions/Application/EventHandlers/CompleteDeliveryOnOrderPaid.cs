using Kofi.Common.Abstractions;
using Kofi.Ordering.Contracts;
using Kofi.Subscriptions.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Subscriptions.Application.EventHandlers;

public class CompleteDeliveryOnOrderPaid(ISubscriptionsDbContext db) : IIntegrationEventHandler<OrderPaidIntegrationEvent>
{
    public async Task Handle(OrderPaidIntegrationEvent paid, CancellationToken ct)
    {
        // An order from the web shop has no delivery.
        if (paid.DeliveryId is not { } deliveryId)
        {
            return;
        }

        var subscription = await db.Subscriptions.SingleAsync(s => s.Deliveries.Any(d => d.Id == deliveryId), ct);
        subscription.CompleteDelivery(deliveryId, paid.OrderId);
    }
}
