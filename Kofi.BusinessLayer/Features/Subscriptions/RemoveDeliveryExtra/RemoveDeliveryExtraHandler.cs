using Kofi.BusinessLayer.Abstractions;
using Kofi.DataAccessLayer.Repositories;
using Kofi.DataAccessLayer.Models;
namespace Kofi.BusinessLayer.Features.Subscriptions.RemoveDeliveryExtra;

public class RemoveDeliveryExtraHandler(ISubscriptionRepository subscriptions) : ICommandHandler<RemoveDeliveryExtraCommand>
{
    public async Task Handle(RemoveDeliveryExtraCommand command, CancellationToken ct)
    {
        var subscription = await subscriptions.GetById(command.SubscriptionId) ?? throw new DomainException("Subscription not found");
        if (subscription.Status is not (SubscriptionStatus.Active or SubscriptionStatus.Paused))
            throw new DomainException("Only a running subscription can change a delivery");
        var delivery = subscription.Deliveries.SingleOrDefault(d => d.Id == command.DeliveryId)
            ?? throw new DomainException("Delivery not found");
        if (delivery.Status != DeliveryStatus.Scheduled) throw new DomainException("Only a planned delivery can be changed");
        var extra = delivery.Extras.SingleOrDefault(e => e.ProductId == command.ProductId)
            ?? throw new DomainException("This delivery has no such extra");
        delivery.Extras.Remove(extra);
    }
}
