using Kofi.Common.Abstractions;
using Kofi.Subscriptions.Application.Repositories;
using Kofi.Subscriptions.Application.Services;

namespace Kofi.Subscriptions.Application.Features.Subscriptions.PrepareDelivery;

// The delivery is announced as due; whoever fulfils it reports back by event.
public class PrepareDeliveryHandler(ISubscriptionRepository subscriptions, IDeliveryProducts products)
    : ICommandHandler<PrepareDeliveryCommand>
{
    public async Task Handle(PrepareDeliveryCommand command, CancellationToken ct)
    {
        var subscription = await subscriptions.GetById(command.SubscriptionId) ?? throw new NotFoundException("Subscription not found");

        if (subscription.Deliveries.SingleOrDefault(d => d.Id == command.DeliveryId) is { } delivery)
        {
            await products.EnsureOnSale(delivery, ct);
        }

        subscription.StartDelivery(command.DeliveryId, DateTime.Now);
    }
}
