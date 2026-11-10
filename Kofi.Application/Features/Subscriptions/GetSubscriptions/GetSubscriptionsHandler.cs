using Kofi.Application.Persistence;
using Kofi.Common.Abstractions;
using Kofi.Common.Services;
using Kofi.Ordering.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Application.Features.Subscriptions.GetSubscriptions;

public class GetSubscriptionsHandler(IApplicationDbContext db, IOrdering ordering, ICurrentUser user)
    : IQueryHandler<GetSubscriptionsQuery, List<SubscriptionSummary>>
{
    public async Task<List<SubscriptionSummary>> Handle(GetSubscriptionsQuery query, CancellationToken ct)
    {
        var q = db.Subscriptions.AsNoTracking().AsQueryable();

        if (user.CustomerId is { } customerId)
        {
            q = q.Where(s => s.CustomerId == customerId);
        }

        var rows = await q.Select(s => new SubscriptionSummary
            {
                Id = s.Id,
                CustomerId = s.CustomerId,
                CustomerName = "",
                ProductName = s.Plan.ProductName,
                Status = s.Status.ToString(),
                NextDeliveryAt = s.NextDeliveryAt
            })
            .ToListAsync(ct);
        var names = await ordering.GetCustomerNames(rows.Select(s => s.CustomerId).Distinct().ToList(), ct);

        return rows.Select(s => s with { CustomerName = names.GetValueOrDefault(s.CustomerId, "") }).ToList();
    }
}
