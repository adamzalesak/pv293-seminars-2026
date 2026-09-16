using Kofi.Application.Abstractions;
using Kofi.Application.Repositories;
using Kofi.Application.Services;

namespace Kofi.Application.Features.Orders.MarkOrderShipped;

public class MarkOrderShippedHandler(IOrderRepository orders, IEmailService email) : ICommandHandler<MarkOrderShippedCommand>
{
    public async Task Handle(MarkOrderShippedCommand command, CancellationToken ct)
    {
        var order = await orders.GetById(command.Id) ?? throw new NotFoundException("Order not found");

        order.Ship();
        await email.SendShippingNotification(order);
    }
}
