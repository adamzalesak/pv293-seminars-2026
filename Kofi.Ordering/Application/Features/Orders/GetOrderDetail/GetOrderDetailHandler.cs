using Kofi.Common.Abstractions;
using Kofi.Common.Services;
using Kofi.Ordering.Application.Persistence;
using Kofi.Ordering.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Ordering.Application.Features.Orders.GetOrderDetail;

public class GetOrderDetailHandler(IOrderingDbContext db, ICurrentUser user, IPricingService pricing)
    : IQueryHandler<GetOrderDetailQuery, OrderDetail?>
{
    public async Task<OrderDetail?> Handle(GetOrderDetailQuery query, CancellationToken ct)
    {
        var detail = await db.Orders.AsNoTracking()
            .Where(o => o.Id == query.Id && (user.CustomerId == null || o.CustomerId == user.CustomerId))
            .Select(o => new OrderDetail
            {
                Id = o.Id,
                CustomerName = o.CustomerName,
                CustomerEmail = o.CustomerEmail.Value,
                Street = o.ShippingAddress.Street,
                City = o.ShippingAddress.City,
                Zip = o.ShippingAddress.Zip,
                Status = o.Status.ToString(),
                CreatedAt = o.CreatedAt,
                ItemsTotal = o.Items.Sum(i => i.UnitPrice.Amount * i.Quantity.Value),
                Shipping = 0,
                Total = 0,
                Items = o.Items.Select(i => new OrderDetailItem
                    {
                        Id = i.Id,
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        UnitPrice = i.UnitPrice.Amount,
                        Currency = i.UnitPrice.Currency,
                        Quantity = i.Quantity.Value
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(ct);
        if (detail == null)
        {
            return null;
        }

        var shipping = pricing.CalculateShipping(detail.ItemsTotal);
        return detail with { Shipping = shipping, Total = Math.Round(detail.ItemsTotal + shipping, 2) };
    }
}
