using Kofi.Catalog.Application.Persistence;
using Kofi.Common.Abstractions;
using Kofi.Common.Demo;
using Kofi.Subscriptions.Contracts;
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

        await ProductSubscriptionCopy.Apply(db, activated.SubscriptionId, activated.Current, ct);
    }
}
