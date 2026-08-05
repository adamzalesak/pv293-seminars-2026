using Microsoft.EntityFrameworkCore;
using Kofi.BusinessLayer.Abstractions;
using Kofi.BusinessLayer.Services;
using Kofi.DataAccessLayer.Models;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Features.Orders.GetOrders;

public class GetOrdersHandler(
    IOrderRepository orders,
    ICurrentUser user,
    IPricingService pricing) : IQueryHandler<GetOrdersQuery, List<OrderSummary>>
{
    public async Task<List<OrderSummary>> Handle(GetOrdersQuery query, CancellationToken ct)
    {
        var q = orders.Query();

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
            "total" => q.OrderByDescending(o => o.Items.Sum(i => i.UnitPrice * i.Quantity)),
            "customer" => q.OrderBy(o => o.CustomerName),
            _ => q.OrderByDescending(o => o.CreatedAt),
        };

        // The total is summed in the database, so a listing does not drag
        // every order item across the wire to add it up.
        var rows = await q
            .Select(o => new
            {
                Order = o,
                ItemsTotal = o.Items.Sum(i => i.UnitPrice * i.Quantity),
            })
            .ToListAsync(ct);

        var result = new List<OrderSummary>();
        foreach (var row in rows)
        {
            result.Add(new OrderSummary
            {
                Id = row.Order.Id,
                CustomerName = row.Order.CustomerName,
                Status = row.Order.Status.ToString(),
                CreatedAt = row.Order.CreatedAt,
                Total = Math.Round(row.ItemsTotal + pricing.CalculateShipping(row.ItemsTotal), 2),
            });
        }
        return result;
    }
}
