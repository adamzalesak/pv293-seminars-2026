using Kofi.Ordering.Application.Payments;
using Kofi.Ordering.Application.Repositories;
using Kofi.Ordering.Application.Services;
using Kofi.Ordering.Contracts;
using Kofi.Ordering.Domain.Orders;
using Kofi.Subscriptions.Contracts;
using Kofi.Warehouse.Contracts;
using Microsoft.Extensions.Options;
using Wolverine;
using Wolverine.Persistence.Sagas;

namespace Kofi.Ordering.Application.Sagas;

// The order for one subscription delivery, from the moment the delivery is
// due until it is paid or given up. The saga owns the state of this process:
// Wolverine stores it after every step, in the same transaction as the step's
// own changes and the messages it sends.
//
// ReservingStock ──StockUnavailable──▶ OrderFailed
//   │ StockReserved: place the order, ChargeCard (the saved card, after the commit)
// AwaitingPayment ──paid──▶ OrderPaid
//   │ declined
// Retrying: e-mail the customer, try again a day later (3 tries in all)
//   │ declined for the third time
// Compensating: cancel the order, release the stock ──StockReleased──▶ OrderCancelled
public class SubscriptionOrderSaga : Saga
{
    public const int MaxPaymentAttempts = 3;

    // The delivery's id: every message of the process carries it.
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public List<SubscriptionOrderLine> Lines { get; set; } = [];
    public SubscriptionOrderStep Step { get; set; }
    public Guid? OrderId { get; set; }
    public int PaymentAttempts { get; set; }

    public static (SubscriptionOrderSaga, ReserveStock) Start(DeliveryDueIntegrationEvent due)
    {
        var saga = new SubscriptionOrderSaga
        {
            Id = due.DeliveryId,
            CustomerId = due.CustomerId,
            Lines = due.Lines.Select(l => new SubscriptionOrderLine(l.ProductId, l.ProductName, l.Quantity, l.UnitPrice, l.Currency))
                .ToList(),
            Step = SubscriptionOrderStep.ReservingStock,
        };

        return (saga, new ReserveStock(due.DeliveryId, saga.Lines.Select(l => new StockLine(l.ProductId, l.Quantity)).ToList()));
    }

    public OrderFailedIntegrationEvent? Handle(
        [SagaIdentityFrom(nameof(StockUnavailableIntegrationEvent.DeliveryId))] StockUnavailableIntegrationEvent unavailable)
    {
        if (Step != SubscriptionOrderStep.ReservingStock)
        {
            return null;
        }

        MarkCompleted();
        return new OrderFailedIntegrationEvent(Id, unavailable.Reason);
    }

    // The goods are put aside, so the order can be placed and paid for. The
    // amount is Ordering's to calculate, which is why the order comes first.
    // The delivery's id names its order too: a step that runs again places
    // the same order and asks for the same payment.
    public async Task<ChargeCard?> Handle(
        [SagaIdentityFrom(nameof(StockReservedIntegrationEvent.DeliveryId))] StockReservedIntegrationEvent reserved,
        IOrderRepository orders, ICustomerRepository customers, IPricingService pricing)
    {
        if (Step != SubscriptionOrderStep.ReservingStock)
        {
            return null;
        }

        var customer = await customers.GetById(CustomerId) ?? throw new InvalidOperationException("Customer not found");
        var order = Order.Create(Id, customer.Id, customer.Name, customer.Email, customer.ShippingAddress, DateTime.Now,
            Lines.Select(l => OrderItem.Create(l.ProductId, l.ProductName, new Quantity(l.Quantity), new Price(l.UnitPrice, l.Currency))),
            deliveryId: Id);
        orders.Add(order);
        OrderId = order.Id;

        return await NextCharge(order, customers, pricing);
    }

    public async Task<object?> Handle([SagaIdentityFrom(nameof(DeliveryPaymentDecided.DeliveryId))] DeliveryPaymentDecided result,
        IOrderRepository orders, IOrderEmailService email, IOptions<SubscriptionOrderOptions> options)
    {
        var order = await orders.GetById(OrderId!.Value) ?? throw new InvalidOperationException("Order not found");

        // Attempts follow one another, so a result is for the attempt we wait
        // for; one that arrives when we wait for nothing changes nothing.
        if (Step != SubscriptionOrderStep.AwaitingPayment)
        {
            return null;
        }

        if (result.Captured)
        {
            // OrderPaid leaves with this step; Subscriptions marks the delivery Ready.
            order.RecordPayment(result.PaymentId);
            MarkCompleted();
            return null;
        }

        if (PaymentAttempts < MaxPaymentAttempts)
        {
            Step = SubscriptionOrderStep.Retrying;
            await email.SendPaymentDeclined(order);
            return new RetryPayment(Id).DelayedFor(options.Value.PaymentRetryAfter);
        }

        // Nothing to roll back across modules: the reservation is undone by a
        // new action, and the order is cancelled.
        Step = SubscriptionOrderStep.Compensating;
        order.Cancel();
        return new ReleaseStock(Id);
    }

    // The customer may have changed the card meanwhile; the saved one is used.
    public async Task<ChargeCard?> Handle([SagaIdentityFrom(nameof(RetryPayment.DeliveryId))] RetryPayment retry,
        IOrderRepository orders, ICustomerRepository customers, IPricingService pricing)
    {
        if (Step != SubscriptionOrderStep.Retrying)
        {
            return null;
        }

        var order = await orders.GetById(OrderId!.Value) ?? throw new InvalidOperationException("Order not found");
        return await NextCharge(order, customers, pricing);
    }

    public OrderCancelledIntegrationEvent? Handle(
        [SagaIdentityFrom(nameof(StockReleasedIntegrationEvent.DeliveryId))] StockReleasedIntegrationEvent released)
    {
        if (Step != SubscriptionOrderStep.Compensating)
        {
            return null;
        }

        MarkCompleted();
        return new OrderCancelledIntegrationEvent(OrderId!.Value, Id, "The payment was declined three times");
    }

    // Messages that reach a process already finished change nothing.
    public static void NotFound(StockReservedIntegrationEvent reserved) { }
    public static void NotFound(StockUnavailableIntegrationEvent unavailable) { }
    public static void NotFound(DeliveryPaymentDecided result) { }
    public static void NotFound(RetryPayment retry) { }
    public static void NotFound(StockReleasedIntegrationEvent released) { }

    // The saga only decides to charge; ChargeCard calls the provider after
    // this step commits. Each attempt has its own idempotency key: the same
    // attempt sent again is the same payment, a new attempt a new charge.
    private async Task<ChargeCard> NextCharge(Order order, ICustomerRepository customers, IPricingService pricing)
    {
        var customer = await customers.GetById(CustomerId) ?? throw new InvalidOperationException("Customer not found");
        var token = customer.PaymentMethodToken ?? throw new InvalidOperationException("The customer has no saved card");

        PaymentAttempts++;
        Step = SubscriptionOrderStep.AwaitingPayment;
        var itemsTotal = pricing.CalculateItemsTotal(order.Items);
        return new ChargeCard(order.Id, Math.Round(itemsTotal + pricing.CalculateShipping(itemsTotal), 2), "EUR", token,
            $"{Id}/{PaymentAttempts}");
    }
}
