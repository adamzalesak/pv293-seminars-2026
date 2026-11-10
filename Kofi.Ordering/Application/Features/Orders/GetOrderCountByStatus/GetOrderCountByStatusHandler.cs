using Kofi.Common.Abstractions;
using Kofi.Ordering.Application.Persistence;
using Kofi.Ordering.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Ordering.Application.Features.Orders.GetOrderCountByStatus;

public class GetOrderCountByStatusHandler(IOrderingDbContext db) : IQueryHandler<GetOrderCountByStatusQuery, int>
{
    public async Task<int> Handle(GetOrderCountByStatusQuery query, CancellationToken ct)
    {
        if (!Enum.TryParse<OrderStatus>(query.Status, true, out var status))
        {
            return 0;
        }

        return await db.Orders.CountAsync(o => o.Status == status, ct);
    }
}
