using Kofi.BusinessLayer.Abstractions;
using Kofi.BusinessLayer.Services;
using Microsoft.EntityFrameworkCore;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Features.Subscriptions.GetSubscriptions;

public class GetSubscriptionsHandler(ISubscriptionRepository subscriptions, ICurrentUser user)
    : IQueryHandler<GetSubscriptionsQuery, List<SubscriptionSummary>>
{
    public Task<List<SubscriptionSummary>> Handle(GetSubscriptionsQuery query, CancellationToken ct)
    {
        var rows = subscriptions.Query();
        // A customer sees their own subscriptions, the administrator everyone's.
        if (user.CustomerId.HasValue)
            rows = rows.Where(s => s.CustomerId == user.CustomerId.Value);
        return rows.OrderBy(s => s.CreatedAt).Select(s => new SubscriptionSummary
        {
            Id = s.Id, CustomerId = s.CustomerId, CustomerName = s.Customer.Name,
            ProductName = s.ProductName, Status = s.Status.ToString(), NextDeliveryAt = s.NextDeliveryAt,
        }).ToListAsync(ct);
    }
}
