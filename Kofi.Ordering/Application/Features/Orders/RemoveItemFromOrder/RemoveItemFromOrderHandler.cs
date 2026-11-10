using Kofi.Common.Abstractions;
using Kofi.Ordering.Application.Ports;
using Kofi.Ordering.Application.Repositories;

namespace Kofi.Ordering.Application.Features.Orders.RemoveItemFromOrder;

public class RemoveItemFromOrderHandler(IOrderRepository orders, IOrderStock stock) : ICommandHandler<RemoveItemFromOrderCommand>
{
    public async Task Handle(RemoveItemFromOrderCommand command, CancellationToken ct)
    {
        var order = await orders.GetById(command.OrderId) ?? throw new NotFoundException("Order not found");

        var item = order.RemoveItem(command.OrderItemId);
        await stock.Release(item.ProductId, item.Quantity.Value);
    }
}
