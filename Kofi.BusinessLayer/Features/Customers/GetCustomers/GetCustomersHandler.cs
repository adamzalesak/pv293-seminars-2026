using Kofi.BusinessLayer.Abstractions;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Features.Customers.GetCustomers;

public class GetCustomersHandler(ICustomerRepository customers)
    : IQueryHandler<GetCustomersQuery, List<CustomerSummary>>
{
    public async Task<List<CustomerSummary>> Handle(GetCustomersQuery query, CancellationToken ct)
    {
        var list = await customers.GetAll();
        return list.Select(c => new CustomerSummary
        {
            Id = c.Id,
            Name = c.Name,
            Email = c.Email,
            IsVip = c.IsVip,
        }).ToList();
    }
}
