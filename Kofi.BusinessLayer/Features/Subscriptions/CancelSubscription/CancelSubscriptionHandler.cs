using Kofi.BusinessLayer.Abstractions;
using Kofi.DataAccessLayer.Models;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Features.Subscriptions.CancelSubscription;

public class CancelSubscriptionHandler(ISubscriptionRepository subscriptions) : ICommandHandler<CancelSubscriptionCommand>
{
    public async Task Handle(CancelSubscriptionCommand command, CancellationToken ct)
    {
        var subscription = await subscriptions.GetById(command.Id)
            ?? throw new DomainException("Subscription not found");
        if (subscription.Status == SubscriptionStatus.Cancelled)
            throw new DomainException("Subscription is already cancelled");
        subscription.Status = SubscriptionStatus.Cancelled;
        subscription.CancelledAt = DateTime.Now;
        subscription.NextDeliveryAt = null;
        foreach (var delivery in subscription.Deliveries.Where(d => d.Status == DeliveryStatus.Scheduled))
            delivery.Status = DeliveryStatus.Cancelled;
    }
}
