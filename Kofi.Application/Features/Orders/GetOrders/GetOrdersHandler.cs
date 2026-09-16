using Kofi.Application.Abstractions;
using Kofi.Application.Persistence;
using Kofi.Application.Services;
using Kofi.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Application.Features.Orders.GetOrders;

public class GetOrdersHandler(
    IApplicationDbContext db,
    ICurrentUser user,
    IPricingService pricing) : IQueryHandler<GetOrdersQuery, List<OrderSummary>>
{
    public async Task<List<OrderSummary>> Handle(GetOrdersQuery query, CancellationToken ct)
    {
        var q = db.Orders.AsNoTracking();

        // A customer sees their own orders, the administrator everyone's.
        if (user.CustomerId != null)
        {
            q = q.Where(o => o.CustomerId == user.CustomerId);
        }

        if (query.OnlyPaid == true)
        {
            q = q.Where(o => o.Status == OrderStatus.Paid || o.Status == OrderStatus.Shipped);
        }

        if (query.From != null)
        {
            q = q.Where(o => o.CreatedAt >= query.From.Value);
        }

        q = query.SortBy switch
        {
            "total" => q.OrderByDescending(o => o.Items.Sum(i => i.UnitPrice.Amount * i.Quantity.Value)),
            "customer" => q.OrderBy(o => o.CustomerName),
            _ => q.OrderByDescending(o => o.CreatedAt),
        };

        // The total is summed in the database, so a listing does not drag
        // every order item across the wire to add it up.
        var rows = await q
            .Select(o => new
            {
                o.Id,
                o.CustomerName,
                o.Status,
                o.CreatedAt,
                ItemsTotal = o.Items.Sum(i => i.UnitPrice.Amount * i.Quantity.Value),
            })
            .ToListAsync(ct);

        var result = new List<OrderSummary>();

        foreach (var row in rows)
        {
            result.Add(new OrderSummary
            {
                Id = row.Id,
                CustomerName = row.CustomerName,
                Status = row.Status.ToString(),
                CreatedAt = row.CreatedAt,
                Total = Math.Round(row.ItemsTotal + pricing.CalculateShipping(row.ItemsTotal), 2),
            });
        }

        return result;
    }
}
