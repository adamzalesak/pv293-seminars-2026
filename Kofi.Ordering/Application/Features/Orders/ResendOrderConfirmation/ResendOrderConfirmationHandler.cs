using Kofi.Common.Abstractions;
using Kofi.Ordering.Application.Repositories;
using Kofi.Ordering.Application.Services;

namespace Kofi.Ordering.Application.Features.Orders.ResendOrderConfirmation;

public class ResendOrderConfirmationHandler(
    IOrderRepository orders,
    IOrderEmailService email) : ICommandHandler<ResendOrderConfirmationCommand>
{
    public async Task Handle(ResendOrderConfirmationCommand command, CancellationToken ct)
    {
        var order = await orders.GetById(command.Id)
                    ?? throw new NotFoundException("Order not found");

        await email.SendOrderConfirmation(order);
    }
}
