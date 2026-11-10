using Kofi.Application.Abstractions;
using Kofi.Application.Persistence;
using Kofi.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Application.Features.Orders.GetOrderCountByStatus;

public class GetOrderCountByStatusHandler(IApplicationDbContext db) : IQueryHandler<GetOrderCountByStatusQuery, int>
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
