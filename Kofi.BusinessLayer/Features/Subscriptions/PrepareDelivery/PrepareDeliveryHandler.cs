using Kofi.BusinessLayer.Abstractions;
using Kofi.BusinessLayer.Services;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Features.Subscriptions.PrepareDelivery;

public class PrepareDeliveryHandler(ISubscriptionRepository subscriptions, ISubscriptionOrderService orders) : ICommandHandler<PrepareDeliveryCommand>
{
    public async Task Handle(PrepareDeliveryCommand command, CancellationToken ct)
    {
        var subscription = await subscriptions.GetById(command.SubscriptionId) ?? throw new NotFoundException("Subscription not found");
        var now = DateTime.Now;
        var prepared = subscription.PrepareDelivery(command.DeliveryId, Guid.NewGuid(), now);
        await orders.Create(subscription.CustomerId, now, prepared);
    }
}
