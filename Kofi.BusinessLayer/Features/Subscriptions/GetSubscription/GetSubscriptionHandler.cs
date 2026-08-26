using Kofi.BusinessLayer.Abstractions;
using Microsoft.EntityFrameworkCore;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Features.Subscriptions.GetSubscription;

public class GetSubscriptionHandler(ISubscriptionRepository subscriptions, IOrderRepository orders)
    : IQueryHandler<GetSubscriptionQuery, SubscriptionDetail?>
{
    public async Task<SubscriptionDetail?> Handle(GetSubscriptionQuery query, CancellationToken ct)
    {
        var detail = await subscriptions.Query().Where(s => s.Id == query.Id).Select(s => new SubscriptionDetail
        {
            Id = s.Id, CustomerId = s.CustomerId, ProductId = s.ProductId,
            ProductName = s.ProductName, Status = s.Status.ToString(), Quantity = s.Quantity,
            UnitPrice = s.UnitPrice, Currency = s.Currency, IntervalDays = s.IntervalDays,
            CreatedAt = s.CreatedAt, ActivatedAt = s.ActivatedAt,
            PausedAt = s.PausedAt, CancelledAt = s.CancelledAt, NextDeliveryAt = s.NextDeliveryAt,
            Deliveries = s.Deliveries.OrderBy(d => d.ScheduledFor).Select(d => new SubscriptionDeliveryDetail
            {
                Extras = d.Extras.Select(e => new SubscriptionExtraDetail
                {
                    ProductId = e.ProductId, ProductName = e.ProductName, Quantity = e.Quantity,
                    UnitPrice = e.UnitPrice, Currency = e.Currency,
                }).ToList(),
                Id = d.Id, ProductId = d.ProductId, Quantity = d.Quantity, UnitPrice = d.UnitPrice,
                Currency = d.Currency, ScheduledFor = d.ScheduledFor,
                OrderId = d.OrderId, OrderStatus = null, Status = d.Status.ToString(),
            }).ToList(),
        }).FirstOrDefaultAsync(ct);
        if (detail == null)
            return null;

        // A delivery ends when its order is placed; from then on the order
        // tells how the parcel is doing, so we read its status alongside.
        var orderIds = detail.Deliveries.Where(d => d.OrderId != null).Select(d => d.OrderId!.Value).ToList();
        var statuses = await orders.Query().Where(o => orderIds.Contains(o.Id))
            .ToDictionaryAsync(o => o.Id, o => o.Status.ToString(), ct);
        return detail with
        {
            Deliveries = detail.Deliveries
                .Select(d => d with { OrderStatus = d.OrderId is { } id ? statuses.GetValueOrDefault(id) : null })
                .ToList(),
        };
    }
}
