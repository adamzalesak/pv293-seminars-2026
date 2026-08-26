using Kofi.BusinessLayer.Abstractions;
using Kofi.DataAccessLayer.Models;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Features.Subscriptions.ResumeSubscription;

public class ResumeSubscriptionHandler(ISubscriptionRepository subscriptions) : ICommandHandler<ResumeSubscriptionCommand>
{
    public async Task Handle(ResumeSubscriptionCommand command, CancellationToken ct)
    {
        var subscription = await subscriptions.GetById(command.Id)
            ?? throw new DomainException("Subscription not found");
        if (subscription.Status != SubscriptionStatus.Paused)
            throw new DomainException("Only paused subscriptions can be resumed");

        var next = command.NextDeliveryOn.ToDateTime(TimeOnly.MinValue);
        subscription.Status = SubscriptionStatus.Active;
        subscription.PausedAt = null;
        subscription.NextDeliveryAt = next;
        foreach (var delivery in subscription.Deliveries.Where(d => d.Status == DeliveryStatus.Scheduled))
            delivery.ScheduledFor = next;
    }
}
