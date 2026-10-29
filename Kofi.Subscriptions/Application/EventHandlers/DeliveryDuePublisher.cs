using Kofi.Common.Abstractions;
using Kofi.Subscriptions.Contracts;
using Kofi.Subscriptions.Domain.Subscriptions;
using Wolverine;

namespace Kofi.Subscriptions.Application.EventHandlers;

public class DeliveryDuePublisher(IMessageBus bus) : IDomainEventHandler<DeliveryDue>
{
    public Task Handle(DeliveryDue due, CancellationToken ct) =>
        bus.PublishAsync(new DeliveryDueIntegrationEvent(due.DeliveryId, due.SubscriptionId, due.CustomerId,
            due.Lines.Select(l => new DeliveryDueLine(l.ProductId, l.ProductName, l.Quantity.Value, l.UnitPrice.Amount,
                l.UnitPrice.Currency)).ToList(), due.DueAt)).AsTask();
}
