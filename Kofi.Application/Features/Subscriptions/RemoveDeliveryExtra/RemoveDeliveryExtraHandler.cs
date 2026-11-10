using Kofi.Application.Repositories;
using Kofi.Common.Abstractions;

namespace Kofi.Application.Features.Subscriptions.RemoveDeliveryExtra;

public class RemoveDeliveryExtraHandler(ISubscriptionRepository subscriptions) : ICommandHandler<RemoveDeliveryExtraCommand>
{
    public async Task Handle(RemoveDeliveryExtraCommand command, CancellationToken ct)
    {
        var subscription = await subscriptions.GetById(command.SubscriptionId) ?? throw new NotFoundException("Subscription not found");

        subscription.RemoveExtra(command.DeliveryId, command.ProductId);
    }
}
