using Kofi.Common.Abstractions;
using Kofi.Subscriptions.Application.Repositories;

namespace Kofi.Subscriptions.Application.Features.Subscriptions.RemoveDeliveryExtra;

public class RemoveDeliveryExtraHandler(ISubscriptionRepository subscriptions) : ICommandHandler<RemoveDeliveryExtraCommand>
{
    public async Task Handle(RemoveDeliveryExtraCommand command, CancellationToken ct)
    {
        var subscription = await subscriptions.GetById(command.SubscriptionId) ?? throw new NotFoundException("Subscription not found");

        subscription.RemoveExtra(command.DeliveryId, command.ProductId);
    }
}
