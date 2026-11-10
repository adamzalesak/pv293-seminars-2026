using Kofi.Common.Abstractions;
using Kofi.Common.Services;
using Kofi.Ordering.Application.Ports;
using Kofi.Ordering.Application.Repositories;
using Kofi.Ordering.Application.Services;

namespace Kofi.Ordering.Application.Features.Orders.CancelOrder;

public class CancelOrderHandler(IOrderRepository orders, IOrderStock stock, IOrderEmailService email, IPaymentGatewayClient payments)
    : ICommandHandler<CancelOrderCommand>
{
    public async Task Handle(CancelOrderCommand command, CancellationToken ct)
    {
        var order = await orders.GetById(command.Id) ?? throw new NotFoundException("Order not found");

        order.Cancel();

        if (order.PaymentId is { } paymentId)
        {
            await payments.CancelOrRefund(paymentId);
        }

        foreach (var item in order.Items)
        {
            await stock.Release(item.ProductId, item.Quantity.Value);
        }

        await email.SendOrderCancelled(order);
    }
}
