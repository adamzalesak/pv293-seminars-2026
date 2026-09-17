using Kofi.Common.Abstractions;
using Kofi.Ordering.Application.Repositories;

namespace Kofi.Ordering.Application.Features.Orders.UpdateShippingAddress;

public class UpdateShippingAddressHandler(IOrderRepository orders) : ICommandHandler<UpdateShippingAddressCommand>
{
    public async Task Handle(UpdateShippingAddressCommand command, CancellationToken ct)
    {
        var order = await orders.GetById(command.Id) ?? throw new NotFoundException("Order not found");

        order.ChangeAddress(new ShippingAddress(command.Street, command.City, command.Zip));
    }
}
