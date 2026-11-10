using Kofi.Application.Repositories;
using Kofi.Application.Services;
using Kofi.Common.Abstractions;

namespace Kofi.Application.Features.Subscriptions.PrepareDelivery;

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
