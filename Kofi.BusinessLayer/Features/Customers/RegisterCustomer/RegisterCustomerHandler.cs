using Kofi.BusinessLayer.Abstractions;
using Kofi.DataAccessLayer.Models;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Features.Customers.RegisterCustomer;

public class RegisterCustomerHandler(ICustomerRepository customers)
    : ICommandHandler<RegisterCustomerCommand, RegisterCustomerResponse>
{
    public async Task<RegisterCustomerResponse> Handle(RegisterCustomerCommand command, CancellationToken ct)
    {
        if (await customers.EmailTaken(command.Email))
        {
            throw new DomainException("A customer with this e-mail already exists");
        }

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Email = command.Email,
            Street = command.Street,
            City = command.City,
            Zip = command.Zip,
            RegisteredAt = DateTime.Now,
            IsVip = false,
        };

        customers.Add(customer);

        return new RegisterCustomerResponse { Id = customer.Id, Email = customer.Email };
    }
}
