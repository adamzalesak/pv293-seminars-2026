using Kofi.BusinessLayer.Abstractions;
using Kofi.DataAccessLayer.Models;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Features.Subscriptions.SkipDelivery;

public class SkipDeliveryHandler(ISubscriptionRepository subscriptions) : ICommandHandler<SkipDeliveryCommand>
{
    public async Task Handle(SkipDeliveryCommand command, CancellationToken ct)
    {
        var subscription = await subscriptions.GetById(command.Id)
            ?? throw new DomainException("Subscription not found");
        if (subscription.Status != SubscriptionStatus.Active)
            throw new DomainException("Only active subscriptions can skip a delivery");
        var delivery = subscription.Deliveries.Where(d => d.ScheduledFor >= DateTime.Today)
            .OrderBy(d => d.ScheduledFor).FirstOrDefault()
            ?? throw new DomainException("No delivery is scheduled");

        delivery.Status = DeliveryStatus.Skipped;
        subscription.NextDeliveryAt = delivery.ScheduledFor.AddDays(subscription.IntervalDays);
        subscription.Deliveries.Add(new SubscriptionDelivery
        {
            Id = Guid.NewGuid(), SubscriptionId = subscription.Id, ProductId = subscription.ProductId,
            Quantity = subscription.Quantity, UnitPrice = subscription.UnitPrice,
            Currency = subscription.Currency, ScheduledFor = subscription.NextDeliveryAt.Value,
        });
    }
}
