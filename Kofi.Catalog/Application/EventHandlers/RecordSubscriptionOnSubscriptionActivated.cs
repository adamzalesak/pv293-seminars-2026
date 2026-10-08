using Kofi.Catalog.Application.Persistence;
using Kofi.Catalog.Application.ReadModels;
using Kofi.Common.Abstractions;
using Kofi.Subscriptions.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Catalog.Application.EventHandlers;

public class RecordSubscriptionOnSubscriptionActivated(ICatalogDbContext db)
    : IIntegrationEventHandler<SubscriptionActivatedIntegrationEvent>
{
    public async Task Handle(SubscriptionActivatedIntegrationEvent activated, CancellationToken ct)
    {
        var copy = await db.ProductSubscriptions.FirstOrDefaultAsync(s => s.SubscriptionId == activated.SubscriptionId, ct);
        if (copy is null)
        {
            copy = new ProductSubscription { SubscriptionId = activated.SubscriptionId };
            db.ProductSubscriptions.Add(copy);
        }

        copy.ProductId = activated.ProductId;
        copy.IsActive = true;
    }
}
