using Kofi.Application.Abstractions;

namespace Kofi.Application.Features.Customers.GetCustomers;

public record GetCustomersQuery : IQuery<List<CustomerSummary>>;
