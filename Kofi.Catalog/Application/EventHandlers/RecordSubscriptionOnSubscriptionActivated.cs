using Kofi.Catalog.Application.Persistence;
using Kofi.Catalog.Application.ReadModels;
using Kofi.Common.Abstractions;
using Kofi.Common.Demo;
using Kofi.Subscriptions.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Kofi.Catalog.Application.EventHandlers;

public class RecordSubscriptionOnSubscriptionActivated(ICatalogDbContext db, IOptions<DemoOptions> demo)
    : IIntegrationEventHandler<SubscriptionActivatedIntegrationEvent>
{
    public async Task Handle(SubscriptionActivatedIntegrationEvent activated, CancellationToken ct)
    {
        if (demo.Value.PopularityFails)
        {
            throw new InvalidOperationException("The popularity count is unavailable");
        }

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
