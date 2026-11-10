using Kofi.Application.Abstractions;
using Kofi.Application.Repositories;
using Kofi.Application.Services;

namespace Kofi.Application.Features.Orders.ResendOrderConfirmation;

public class ResendOrderConfirmationHandler(
    IOrderRepository orders,
    IEmailService email) : ICommandHandler<ResendOrderConfirmationCommand>
{
    public async Task Handle(ResendOrderConfirmationCommand command, CancellationToken ct)
    {
        var order = await orders.GetById(command.Id)
                    ?? throw new NotFoundException("Order not found");

        await email.SendOrderConfirmation(order);
    }
}
