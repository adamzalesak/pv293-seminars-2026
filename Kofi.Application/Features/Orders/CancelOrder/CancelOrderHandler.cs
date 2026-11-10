using Kofi.Application.Abstractions;
using Kofi.Application.Repositories;
using Kofi.Application.Services;

namespace Kofi.Application.Features.Orders.CancelOrder;

public class CancelOrderHandler(IOrderRepository orders, IStockService stock, IEmailService email, IPaymentGatewayClient payments)
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
