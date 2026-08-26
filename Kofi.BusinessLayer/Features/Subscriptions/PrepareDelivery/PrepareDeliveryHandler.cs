using Kofi.BusinessLayer.Abstractions;
using Kofi.BusinessLayer.Services;
using Kofi.DataAccessLayer.Repositories;
using Kofi.DataAccessLayer.Models;
namespace Kofi.BusinessLayer.Features.Subscriptions.PrepareDelivery;

public class PrepareDeliveryHandler(ISubscriptionRepository subscriptions, ISubscriptionOrderService orders) : ICommandHandler<PrepareDeliveryCommand>
{
    public async Task Handle(PrepareDeliveryCommand command, CancellationToken ct)
    {
        var subscription = await subscriptions.GetById(command.SubscriptionId) ?? throw new DomainException("Subscription not found");
        var delivery = subscription.Deliveries.SingleOrDefault(d => d.Id == command.DeliveryId)
            ?? throw new DomainException("Delivery not found");
        var now = DateTime.Now;
        var orderId = Guid.NewGuid();
        if (subscription.Status != SubscriptionStatus.Active) throw new DomainException("Only an active subscription can prepare a delivery");
        if (delivery.Status != DeliveryStatus.Scheduled) throw new DomainException("Only a planned delivery can be prepared");
        if (delivery.ScheduledFor > now) throw new DomainException("This delivery is not due yet");
        var lines = new List<SubscriptionOrderLine>
        {
            new(delivery.ProductId, subscription.ProductName, delivery.Quantity, delivery.UnitPrice, delivery.Currency),
        };
        lines.AddRange(delivery.Extras.Select(e => new SubscriptionOrderLine(e.ProductId, e.ProductName, e.Quantity, e.UnitPrice, e.Currency)));
        await orders.Create(subscription.CustomerId, orderId, now, lines);
        delivery.Status = DeliveryStatus.Ready;
        delivery.OrderId = orderId;
        subscription.NextDeliveryAt = delivery.ScheduledFor.AddDays(subscription.IntervalDays);
        subscription.Deliveries.Add(new SubscriptionDelivery
        {
            Id = Guid.NewGuid(), SubscriptionId = subscription.Id, ProductId = subscription.ProductId,
            Quantity = subscription.Quantity, UnitPrice = subscription.UnitPrice, Currency = subscription.Currency,
            ScheduledFor = subscription.NextDeliveryAt.Value,
        });
    }
}
