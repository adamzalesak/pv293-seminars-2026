using Kofi.Application.Abstractions;

namespace Kofi.Application.Features.Customers.GetCustomer;

public record GetCustomerQuery : IQuery<CustomerDetail?>
{
    public Guid Id { get; init; }
}
