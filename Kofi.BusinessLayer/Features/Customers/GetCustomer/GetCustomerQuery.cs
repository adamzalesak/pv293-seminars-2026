using Kofi.BusinessLayer.Abstractions;

namespace Kofi.BusinessLayer.Features.Customers.GetCustomer;

public record GetCustomerQuery : IQuery<CustomerDetail?>
{
    public Guid Id { get; init; }
}
