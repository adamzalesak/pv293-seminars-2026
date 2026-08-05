using Kofi.BusinessLayer.Abstractions;
using Kofi.DataAccessLayer.Models;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Features.Orders.UpdateShippingAddress;

public class UpdateShippingAddressHandler(IOrderRepository orders)
    : ICommandHandler<UpdateShippingAddressCommand>
{
    public async Task Handle(UpdateShippingAddressCommand command, CancellationToken ct)
    {
        var order = await orders.GetById(command.Id)
            ?? throw new DomainException("Order not found");
        if (order.Status != OrderStatus.New && order.Status != OrderStatus.Paid)
        {
            throw new DomainException("The address can only change before the order ships");
        }

        order.Street = command.Street;
        order.City = command.City;
        order.Zip = command.Zip;

        orders.Update(order);
    }
}
