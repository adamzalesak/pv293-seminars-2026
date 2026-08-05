using Kofi.BusinessLayer.Abstractions;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Features.Customers.GetCustomer;

public class GetCustomerHandler(ICustomerRepository customers)
    : IQueryHandler<GetCustomerQuery, CustomerDetail?>
{
    public async Task<CustomerDetail?> Handle(GetCustomerQuery query, CancellationToken ct)
    {
        var customer = await customers.GetById(query.Id);
        if (customer == null)
        {
            return null;
        }

        return new CustomerDetail
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
            Street = customer.Street,
            City = customer.City,
            Zip = customer.Zip,
            RegisteredAt = customer.RegisteredAt,
            IsVip = customer.IsVip,
        };
    }
}
