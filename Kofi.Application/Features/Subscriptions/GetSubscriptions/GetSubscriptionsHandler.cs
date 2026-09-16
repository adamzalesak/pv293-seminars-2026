using Kofi.Application.Abstractions;
using Kofi.Application.Persistence;
using Kofi.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Application.Features.Subscriptions.GetSubscriptions;

public class GetSubscriptionsHandler(IApplicationDbContext db, ICurrentUser user)
    : IQueryHandler<GetSubscriptionsQuery, List<SubscriptionSummary>>
{
    public Task<List<SubscriptionSummary>> Handle(GetSubscriptionsQuery query, CancellationToken ct)
    {
        var rows = db.Subscriptions.AsQueryable();

        // A customer sees their own subscriptions, the administrator everyone's.
        if (user.CustomerId.HasValue)
        {
            rows = rows.Where(s => s.CustomerId == user.CustomerId.Value);
        }

        // The subscription keeps the customer's id, not a navigation property,
        // so the name is joined in here, in one query.
        return rows
            .OrderBy(s => s.CreatedAt)
            .Join(db.Customers, s => s.CustomerId, c => c.Id, (s, c) => new SubscriptionSummary
            {
                Id = s.Id,
                CustomerId = s.CustomerId,
                CustomerName = c.Name,
                ProductName = s.Plan.ProductName,
                Status = s.Status.ToString(),
                NextDeliveryAt = s.NextDeliveryAt,
            })
            .ToListAsync(ct);
    }
}
