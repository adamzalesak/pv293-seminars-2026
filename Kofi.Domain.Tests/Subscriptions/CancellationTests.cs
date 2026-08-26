using Kofi.Domain.Subscriptions;
using Xunit;

namespace Kofi.Domain.Tests.Subscriptions;

public class CancellationTests
{
    [Fact]
    public void Cancelling_after_the_first_delivery_stops_future_deliveries_immediately()
    {
        var subscription = SubscriptionFactory.Active();

        subscription.Cancel(SubscriptionFactory.Now.AddDays(2));

        Assert.Equal(SubscriptionStatus.Cancelled, subscription.Status);
        Assert.Null(subscription.NextDeliveryAt);
        Assert.DoesNotContain(subscription.Deliveries, d => d.Status == DeliveryStatus.Scheduled);
    }

    [Fact]
    public void Pausing_after_the_first_delivery_keeps_the_waiting_delivery()
    {
        var subscription = SubscriptionFactory.Active();
        var next = SubscriptionFactory.NextScheduled(subscription);

        subscription.Pause(SubscriptionFactory.Now.AddDays(2));

        Assert.Equal(SubscriptionStatus.Paused, subscription.Status);
        Assert.Equal(DeliveryStatus.Scheduled, next.Status);
        Assert.Equal(next.ScheduledFor, subscription.NextDeliveryAt);
    }
}
