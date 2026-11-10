using Kofi.Domain.Subscriptions;
using Kofi.Ordering.Contracts;

namespace Kofi.Application.Services;

public class SubscriptionOrderService(IOrdering ordering) : ISubscriptionOrderService
{
    public async Task Prepare(Subscription subscription, Guid deliveryId, DateTime now, CancellationToken ct)
    {
        var lines = subscription.LinesToPrepare(deliveryId, now);
        var order = await ordering.CreateOrder(new CreateOrderRequest(subscription.CustomerId,
            lines.Select(l => new OrderLine(l.ProductId, l.ProductName, l.Quantity.Value, l.UnitPrice.Amount, l.UnitPrice.Currency))
                .ToList()), ct);
        subscription.PrepareDelivery(deliveryId, order.OrderId, now);
    }
}
