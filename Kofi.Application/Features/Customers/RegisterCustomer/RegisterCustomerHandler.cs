using Kofi.Application.Abstractions;
using Kofi.Application.Repositories;
using Kofi.Domain;
using Kofi.Domain.Customers;
using Kofi.Domain.Shared;

namespace Kofi.Application.Features.Customers.RegisterCustomer;

public class RegisterCustomerHandler(ICustomerRepository customers) : ICommandHandler<RegisterCustomerCommand, RegisterCustomerResponse>
{
    public async Task<RegisterCustomerResponse> Handle(RegisterCustomerCommand command, CancellationToken ct)
    {
        var email = new Email(command.Email);
        if (await customers.EmailTaken(email.Value))
        {
            throw new DomainRuleException("A customer with this e-mail already exists");
        }

        var customer = Customer.Register(Guid.NewGuid(), command.Name, email, command.Street, command.City, command.Zip, DateTime.Now);
        customers.Add(customer);

        return new RegisterCustomerResponse { Id = customer.Id, Email = customer.Email.Value };
    }
}
