using Kofi.Application.Abstractions;
using Kofi.Application.Repositories;

namespace Kofi.Application.Features.Orders.UpdateShippingAddress;

public class UpdateShippingAddressHandler(IOrderRepository orders) : ICommandHandler<UpdateShippingAddressCommand>
{
    public async Task Handle(UpdateShippingAddressCommand command, CancellationToken ct)
    {
        var order = await orders.GetById(command.Id) ?? throw new NotFoundException("Order not found");

        order.ChangeAddress(command.Street, command.City, command.Zip);
    }
}
