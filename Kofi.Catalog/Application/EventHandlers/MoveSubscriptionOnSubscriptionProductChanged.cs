using Kofi.Catalog.Application.Persistence;
using Kofi.Common.Abstractions;
using Kofi.Subscriptions.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Catalog.Application.EventHandlers;

public class MoveSubscriptionOnSubscriptionProductChanged(ICatalogDbContext db)
    : IIntegrationEventHandler<SubscriptionProductChangedIntegrationEvent>
{
    public async Task Handle(SubscriptionProductChangedIntegrationEvent changed, CancellationToken ct)
    {
        if (await db.ProductSubscriptions.FirstOrDefaultAsync(s => s.SubscriptionId == changed.SubscriptionId, ct) is { } copy)
        {
            copy.ProductId = changed.ProductId;
        }
    }
}
