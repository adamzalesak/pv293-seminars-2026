using Kofi.Application.Abstractions;
using Kofi.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Application.Features.Subscriptions.GetSubscription;

// A read decides nothing, so it skips the aggregate and its repository and
// projects exactly the columns the screen shows.
public class GetSubscriptionHandler(IApplicationDbContext db)
    : IQueryHandler<GetSubscriptionQuery, SubscriptionDetail?>
{
    public Task<SubscriptionDetail?> Handle(GetSubscriptionQuery query, CancellationToken ct)
    {
        return db.Subscriptions
            .Where(s => s.Id == query.Id)
            .Select(s => new SubscriptionDetail
            {
                Id = s.Id,
                CustomerId = s.CustomerId,
                ProductId = s.Plan.ProductId,
                ProductName = s.Plan.ProductName,
                Status = s.Status.ToString(),
                Quantity = s.Plan.Quantity.Value,
                UnitPrice = s.Plan.UnitPrice.Amount,
                Currency = s.Plan.UnitPrice.Currency,
                IntervalDays = s.Plan.IntervalDays,

                CreatedAt = s.CreatedAt,
                ActivatedAt = s.ActivatedAt,
                PausedAt = s.PausedAt,
                CancelledAt = s.CancelledAt,
                NextDeliveryAt = s.NextDeliveryAt,
                Deliveries = s.Deliveries.OrderBy(d => d.ScheduledFor)
                    .Select(d => new SubscriptionDeliveryDetail
                    {
                        Extras = d.Extras.Select(e => new SubscriptionExtraDetail
                            {
                                ProductId = e.ProductId,
                                ProductName = e.ProductName,
                                Quantity = e.Quantity.Value,
                                UnitPrice = e.UnitPrice.Amount,
                                Currency = e.UnitPrice.Currency,
                            })
                            .ToList(),
                        Id = d.Id,
                        ProductId = d.ProductId,
                        Quantity = d.Quantity.Value,
                        UnitPrice = d.UnitPrice.Amount,
                        Currency = d.UnitPrice.Currency,
                        ScheduledFor = d.ScheduledFor,
                        OrderId = d.OrderId,
                        // A delivery ends when its order is placed; from then on
                        // the order tells how the parcel is doing.
                        OrderStatus = db.Orders.Where(o => o.Id == d.OrderId).Select(o => o.Status.ToString()).FirstOrDefault(),
                        Status = d.Status.ToString(),
                    })
                    .ToList(),
            })
            .AsSplitQuery()
            .FirstOrDefaultAsync(ct);
    }
}
