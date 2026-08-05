using Kofi.BusinessLayer.Abstractions;
using Kofi.DataAccessLayer.Models;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Features.Orders.GetOrderCountByStatus;

public class GetOrderCountByStatusHandler(IOrderRepository orders)
    : IQueryHandler<GetOrderCountByStatusQuery, int>
{
    public async Task<int> Handle(GetOrderCountByStatusQuery query, CancellationToken ct)
    {
        // The endpoint passes whatever the caller typed.
        if (!Enum.TryParse<OrderStatus>(query.Status, ignoreCase: true, out var parsed))
        {
            return 0;
        }

        return await orders.CountByStatus(parsed);
    }
}
