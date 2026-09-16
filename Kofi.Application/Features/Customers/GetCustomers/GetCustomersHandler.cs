using Kofi.Application.Abstractions;
using Kofi.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Application.Features.Customers.GetCustomers;

public class GetCustomersHandler(IApplicationDbContext db) : IQueryHandler<GetCustomersQuery, List<CustomerSummary>>
{
    public async Task<List<CustomerSummary>> Handle(GetCustomersQuery query, CancellationToken ct)
    {
        return await db.Customers.AsNoTracking()
            .Select(c => new CustomerSummary { Id = c.Id, Name = c.Name, Email = c.Email.Value, IsVip = c.IsVip })
            .ToListAsync(ct);
    }
}
