using Kofi.BusinessLayer.Abstractions;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Features.Customers.MakeCustomerVip;

public class MakeCustomerVipHandler(ICustomerRepository customers)
    : ICommandHandler<MakeCustomerVipCommand>
{
    public async Task Handle(MakeCustomerVipCommand command, CancellationToken ct)
    {
        var customer = await customers.GetById(command.Id)
            ?? throw new DomainException("Customer not found");

        customer.IsVip = true;
        customers.Update(customer);
    }
}
