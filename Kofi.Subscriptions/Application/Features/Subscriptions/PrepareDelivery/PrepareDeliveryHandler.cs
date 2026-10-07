using Kofi.Common.Abstractions;
using Kofi.Subscriptions.Application.Repositories;
using Kofi.Subscriptions.Application.Services;

namespace Kofi.Subscriptions.Application.Features.Subscriptions.PrepareDelivery;

public class PrepareDeliveryHandler(ISubscriptionRepository subscriptions, ISubscriptionOrderService orders)
    : ICommandHandler<PrepareDeliveryCommand>
{
    public async Task Handle(PrepareDeliveryCommand command, CancellationToken ct)
    {
        var subscription = await subscriptions.GetById(command.SubscriptionId) ?? throw new NotFoundException("Subscription not found");

        var now = DateTime.Now;
        await orders.Prepare(subscription, command.DeliveryId, now, ct);
    }
}
