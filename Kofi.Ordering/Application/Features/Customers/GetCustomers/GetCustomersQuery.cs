using Kofi.Common.Abstractions;

namespace Kofi.Ordering.Application.Features.Customers.GetCustomers;

public record GetCustomersQuery : IQuery<List<CustomerSummary>>;
