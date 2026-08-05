using Kofi.BusinessLayer.Abstractions;
using Kofi.BusinessLayer.Services;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Features.Orders.ResendOrderConfirmation;

public class ResendOrderConfirmationHandler(
    IOrderRepository orders,
    IEmailService email) : ICommandHandler<ResendOrderConfirmationCommand>
{
    public async Task Handle(ResendOrderConfirmationCommand command, CancellationToken ct)
    {
        var order = await orders.GetByIdWithItems(command.Id)
            ?? throw new DomainException("Order not found");

        await email.SendOrderConfirmation(order);
    }
}
