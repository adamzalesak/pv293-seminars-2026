using Kofi.Common.Abstractions;
using Kofi.Ordering.Application.Repositories;
using Kofi.Ordering.Contracts;
using Kofi.Ordering.Domain.Orders;
using Wolverine;

namespace Kofi.Ordering.Application.EventHandlers;

// The domain event says only that the order was paid; who paid, with which
// coupon and for which delivery is added here, for the other modules.
public class OrderPaidPublisher(IOrderRepository orders, IMessageBus bus) : IDomainEventHandler<OrderPaid>
{
    public async Task Handle(OrderPaid paid, CancellationToken ct)
    {
        var order = await orders.GetById(paid.OrderId) ?? throw new InvalidOperationException("A paid order is missing");
        await bus.PublishAsync(new OrderPaidIntegrationEvent(order.Id, order.CustomerId, order.Coupon?.Code, order.DeliveryId));
    }
}
