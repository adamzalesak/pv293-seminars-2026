using Kofi.BusinessLayer.Abstractions;
using Kofi.BusinessLayer.Services;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Features.Orders.GetOrderDetail;

public class GetOrderDetailHandler(IOrderRepository orders, ICurrentUser user, IPricingService pricing)
    : IQueryHandler<GetOrderDetailQuery, OrderDetail?>
{
    public async Task<OrderDetail?> Handle(GetOrderDetailQuery query, CancellationToken ct)
    {
        // A customer sees only their own order; anyone else's does not exist.
        var order = await orders.GetByIdWithItems(query.Id);
        if (order == null || (user.CustomerId != null && order.CustomerId != user.CustomerId))
        {
            return null;
        }

        var itemsTotal = pricing.CalculateItemsTotal(order.Items);
        var shipping = pricing.CalculateShipping(itemsTotal);

        return new OrderDetail
        {
            Id = order.Id,
            CustomerName = order.CustomerName,
            CustomerEmail = order.CustomerEmail,
            Street = order.Street,
            City = order.City,
            Zip = order.Zip,
            Status = order.Status.ToString(),
            CreatedAt = order.CreatedAt,
            ItemsTotal = Math.Round(itemsTotal, 2),
            Shipping = shipping,
            Total = Math.Round(itemsTotal + shipping, 2),
            Items = order.Items.Select(i => new OrderDetailItem
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
            }).ToList(),
        };
    }
}
