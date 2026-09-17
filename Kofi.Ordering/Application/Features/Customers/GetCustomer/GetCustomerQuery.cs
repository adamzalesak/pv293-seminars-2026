using Kofi.Common.Abstractions;

namespace Kofi.Ordering.Application.Features.Customers.GetCustomer;

public record GetCustomerQuery : IQuery<CustomerDetail?>
{
    public Guid Id { get; init; }
}
