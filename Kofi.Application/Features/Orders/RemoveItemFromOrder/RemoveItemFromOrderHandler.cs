using Kofi.Application.Abstractions;
using Kofi.Application.Repositories;
using Kofi.Application.Services;

namespace Kofi.Application.Features.Orders.RemoveItemFromOrder;

public class RemoveItemFromOrderHandler(IOrderRepository orders, IStockService stock) : ICommandHandler<RemoveItemFromOrderCommand>
{
    public async Task Handle(RemoveItemFromOrderCommand command, CancellationToken ct)
    {
        var order = await orders.GetById(command.OrderId) ?? throw new NotFoundException("Order not found");

        var item = order.RemoveItem(command.OrderItemId);
        await stock.Release(item.ProductId, item.Quantity.Value);
    }
}
