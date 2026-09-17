using Kofi.Common.Abstractions;
using Kofi.Ordering.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Ordering.Application.Features.Customers.GetCustomer;

public class GetCustomerHandler(IOrderingDbContext db) : IQueryHandler<GetCustomerQuery, CustomerDetail?>
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
                Street = c.ShippingAddress.Street,
                City = c.ShippingAddress.City,
                Zip = c.ShippingAddress.Zip,
                RegisteredAt = c.RegisteredAt,
                IsVip = c.IsVip
            })
            .FirstOrDefaultAsync(ct);
    }
}
