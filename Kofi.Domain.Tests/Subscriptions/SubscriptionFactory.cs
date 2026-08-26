using Kofi.Domain.Shared;
using Kofi.Domain.Subscriptions;

namespace Kofi.Domain.Tests.Subscriptions;

// Every test builds its state through public operations. Nothing here
// reaches past the aggregate to set a field directly.
internal static class SubscriptionFactory
{
    public static readonly DateTime Now = new(2026, 9, 1, 10, 0, 0, DateTimeKind.Unspecified);
    public static readonly Guid ProductId = Guid.Parse("f1a0c2b4-0000-0000-0000-000000000001");
    public static readonly Guid FirstOrderId = Guid.Parse("f1a0c2b4-0000-0000-0000-000000000002");

    public static Plan Plan(int quantity = 2, int intervalDays = 21, decimal unitPrice = 12.50m) =>
        new(ProductId, "Etiopie Yirgacheffe 250 g", quantity, intervalDays, new Price(unitPrice, "EUR"));

    // Unless a test says otherwise, the customer wants the first delivery today.
    public static Subscription Incomplete(DateTime? firstDeliveryOn = null) =>
        Subscription.Create(
            customerId: Guid.NewGuid(),
            plan: Plan(),
            firstDeliveryOn: firstDeliveryOn ?? Now.Date,
            now: Now);

    // Activated with the first delivery prepared at once, as the application
    // does when that delivery is due.
    public static Subscription Active()
    {
        var subscription = Incomplete();
        subscription.Activate(Now);
        subscription.PrepareDelivery(subscription.Deliveries.Single().Id, FirstOrderId, Now);
        return subscription;
    }

    public static Subscription Cancelled()
    {
        var subscription = Active();
        subscription.Cancel(Later());
        return subscription;
    }

    // The delivery the first order was placed for.
    public static Delivery FirstDelivery(Subscription subscription) =>
        subscription.Deliveries.Single(d => d.OrderId == FirstOrderId);

    public static Delivery NextScheduled(Subscription subscription) =>
        subscription.Deliveries.Single(d => d.Status == DeliveryStatus.Scheduled);

    public static DateTime Later() =>
        Now.AddDays(32);
}
