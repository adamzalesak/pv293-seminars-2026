using Kofi.Common.Abstractions;
using Kofi.Ordering.Application.Repositories;
using Kofi.Ordering.Application.Services;

namespace Kofi.Ordering.Application.Features.Orders.MarkOrderShipped;

public class MarkOrderShippedHandler(IOrderRepository orders, IOrderEmailService email) : ICommandHandler<MarkOrderShippedCommand>
{
    public async Task Handle(MarkOrderShippedCommand command, CancellationToken ct)
    {
        var order = await orders.GetById(command.Id) ?? throw new NotFoundException("Order not found");

        order.Ship();
        await email.SendShippingNotification(order);
    }
}
