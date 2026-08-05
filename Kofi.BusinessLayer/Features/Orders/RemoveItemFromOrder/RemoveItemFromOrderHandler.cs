using Kofi.BusinessLayer.Abstractions;
using Kofi.BusinessLayer.Services;
using Kofi.DataAccessLayer.Models;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Features.Orders.RemoveItemFromOrder;

public class RemoveItemFromOrderHandler(
    IOrderRepository orders,
    IStockService stock) : ICommandHandler<RemoveItemFromOrderCommand>
{
    public async Task Handle(RemoveItemFromOrderCommand command, CancellationToken ct)
    {
        var order = await orders.GetByIdWithItems(command.OrderId)
            ?? throw new DomainException("Order not found");
        // Only a new order whose payment has not started can change.
        if (order.Status != OrderStatus.New || order.PaymentId != null)
        {
            throw new DomainException("This order can no longer change");
        }

        var item = order.Items.FirstOrDefault(i => i.Id == command.OrderItemId)
            ?? throw new DomainException("Item not found");
        if (order.Items.Count == 1)
        {
            throw new DomainException("An order needs at least one item; cancel it instead");
        }

        order.Items.Remove(item);
        await stock.Release(item.ProductId, item.Quantity);
        orders.Update(order);
    }
}
