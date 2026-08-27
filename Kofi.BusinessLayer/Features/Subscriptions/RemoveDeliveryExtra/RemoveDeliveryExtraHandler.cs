using Kofi.BusinessLayer.Abstractions;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Features.Subscriptions.RemoveDeliveryExtra;

public class RemoveDeliveryExtraHandler(ISubscriptionRepository subscriptions) : ICommandHandler<RemoveDeliveryExtraCommand>
{
    public async Task Handle(RemoveDeliveryExtraCommand command, CancellationToken ct)
    {
        var subscription = await subscriptions.GetById(command.SubscriptionId) ?? throw new NotFoundException("Subscription not found");
        subscription.RemoveExtra(command.DeliveryId, command.ProductId);
    }
}
