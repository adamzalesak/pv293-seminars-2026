using Kofi.Common.Abstractions;
using Kofi.Ordering.Contracts;
using Kofi.Ordering.Domain.Orders;
using MediatR;

namespace Kofi.Ordering.Application.EventHandlers;

public class OrderPaidPublisher(IPublisher publisher) : IDomainEventHandler<OrderPaid>
{
    public Task Handle(OrderPaid paid, CancellationToken ct) =>
        publisher.Publish(new OrderPaidIntegrationEvent(paid.OrderId, paid.CustomerId, paid.CouponCode), ct);
}
