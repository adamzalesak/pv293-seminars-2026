using Kofi.Common.Abstractions;
using Kofi.Ordering.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Ordering.Application.Features.Customers.GetCustomers;

public class GetCustomersHandler(IOrderingDbContext db) : IQueryHandler<GetCustomersQuery, List<CustomerSummary>>
{
    public async Task<List<CustomerSummary>> Handle(GetCustomersQuery query, CancellationToken ct)
    {
        return await db.Customers.AsNoTracking()
            .Select(c => new CustomerSummary { Id = c.Id, Name = c.Name, Email = c.Email.Value, IsVip = c.IsVip })
            .ToListAsync(ct);
    }
}
