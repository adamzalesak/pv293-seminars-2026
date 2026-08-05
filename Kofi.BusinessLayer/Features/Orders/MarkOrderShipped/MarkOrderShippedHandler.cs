using Kofi.BusinessLayer.Abstractions;
using Kofi.BusinessLayer.Services;
using Kofi.DataAccessLayer.Models;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Features.Orders.MarkOrderShipped;

public class MarkOrderShippedHandler(
    IOrderRepository orders,
    IEmailService email) : ICommandHandler<MarkOrderShippedCommand>
{
    public async Task Handle(MarkOrderShippedCommand command, CancellationToken ct)
    {
        var order = await orders.GetById(command.Id)
            ?? throw new DomainException("Order not found");

        if (order.Status != OrderStatus.Paid)
        {
            throw new DomainException("Only paid orders can be shipped");
        }

        order.Status = OrderStatus.Shipped;
        orders.Update(order);

        await email.SendShippingNotification(order);
    }
}
