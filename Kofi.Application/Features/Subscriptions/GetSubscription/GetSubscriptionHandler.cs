using Kofi.Application.Persistence;
using Kofi.Common.Abstractions;
using Kofi.Ordering.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Application.Features.Subscriptions.GetSubscription;

// A read decides nothing, so it skips the aggregate and its repository and
// projects exactly the columns the screen shows.
public class GetSubscriptionHandler(IApplicationDbContext db, IOrdering ordering)
    : IQueryHandler<GetSubscriptionQuery, SubscriptionDetail?>
{
    public async Task<SubscriptionDetail?> Handle(GetSubscriptionQuery query, CancellationToken ct)
    {
        var detail = await db.Subscriptions
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
                        OrderStatus = null,
                        Status = d.Status.ToString(),
                    })
                    .ToList(),
            })
            .AsSplitQuery()
            .FirstOrDefaultAsync(ct);
        if (detail is null)
        {
            return null;
        }

        var statuses =
            await ordering.GetOrderStatuses(detail.Deliveries.Where(d => d.OrderId.HasValue).Select(d => d.OrderId!.Value).ToList(), ct);
        return detail with
        {
            Deliveries = detail.Deliveries
                .Select(d => d with { OrderStatus = d.OrderId is { } id ? statuses.GetValueOrDefault(id) : null })
                .ToList()
        };
    }
}
