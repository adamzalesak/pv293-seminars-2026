using Kofi.Common.Abstractions;
using Kofi.Ordering.Application.Repositories;

namespace Kofi.Ordering.Application.Features.Customers.MakeCustomerVip;

public class MakeCustomerVipHandler(ICustomerRepository customers) : ICommandHandler<MakeCustomerVipCommand>
{
    public async Task Handle(MakeCustomerVipCommand command, CancellationToken ct)
    {
        var customer = await customers.GetById(command.Id) ?? throw new NotFoundException("Customer not found");

        customer.MakeVip();
    }
}
