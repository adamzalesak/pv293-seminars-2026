using Kofi.Application.Abstractions;
using Kofi.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Application.Features.Customers.GetCustomer;

public class GetCustomerHandler(IApplicationDbContext db) : IQueryHandler<GetCustomerQuery, CustomerDetail?>
{
    public async Task<CustomerDetail?> Handle(GetCustomerQuery query, CancellationToken ct)
    {
        return await db.Customers.AsNoTracking()
            .Where(c => c.Id == query.Id)
            .Select(c => new CustomerDetail
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email.Value,
                Street = c.Street,
                City = c.City,
                Zip = c.Zip,
                RegisteredAt = c.RegisteredAt,
                IsVip = c.IsVip
            })
            .FirstOrDefaultAsync(ct);
    }
}
