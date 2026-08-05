using Kofi.BusinessLayer.Abstractions;

namespace Kofi.BusinessLayer.Features.Customers.GetCustomers;

public record GetCustomersQuery : IQuery<List<CustomerSummary>>;
