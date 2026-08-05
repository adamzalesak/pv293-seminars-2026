using Kofi.BusinessLayer.Abstractions;
using Kofi.BusinessLayer.Services;
using Kofi.DataAccessLayer.Models;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Features.Orders.CancelOrder;

public class CancelOrderHandler(
    IOrderRepository orders,
    IStockService stock,
    IEmailService email,
    IPaymentGatewayClient payments) : ICommandHandler<CancelOrderCommand>
{
    public async Task Handle(CancelOrderCommand command, CancellationToken ct)
    {
        var order = await orders.GetByIdWithItems(command.Id)
            ?? throw new DomainException("Order not found");

        if (order.Status == OrderStatus.Cancelled)
        {
            throw new DomainException("The order is already cancelled");
        }
        if (order.Status == OrderStatus.Shipped)
        {
            throw new DomainException("Shipped orders cannot be cancelled");
        }

        // An open payment is stopped, a finished one is refunded.
        if (order.PaymentId != null)
        {
            await payments.CancelOrRefund(order.PaymentId.Value);
        }

        order.Status = OrderStatus.Cancelled;
        orders.Update(order);

        foreach (var item in order.Items)
        {
            await stock.Release(item.ProductId, item.Quantity);
        }

        await email.SendOrderCancelled(order);
    }
}
