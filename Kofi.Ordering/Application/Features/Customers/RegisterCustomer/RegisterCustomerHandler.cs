using Kofi.Common.Abstractions;
using Kofi.Ordering.Application.Repositories;
using Kofi.Ordering.Domain.Customers;

namespace Kofi.Ordering.Application.Features.Customers.RegisterCustomer;

public class RegisterCustomerHandler(ICustomerRepository customers) : ICommandHandler<RegisterCustomerCommand, RegisterCustomerResponse>
{
    public async Task<RegisterCustomerResponse> Handle(RegisterCustomerCommand command, CancellationToken ct)
    {
        var email = new Email(command.Email);
        if (await customers.EmailTaken(email.Value))
        {
            throw new DomainRuleException("A customer with this e-mail already exists");
        }

        var customer = Customer.Register(Guid.NewGuid(), command.Name, email,
            new ShippingAddress(command.Street, command.City, command.Zip), DateTime.Now);
        customers.Add(customer);

        return new RegisterCustomerResponse { Id = customer.Id, Email = customer.Email.Value };
    }
}
