using Kofi.Domain.Subscriptions;
using Xunit;

namespace Kofi.Domain.Tests.Subscriptions;

public class NextDeliveryRulesTests
{
    [Fact]
    public void A_cancelled_subscription_cannot_change_its_plan()
    {
        var sub = SubscriptionFactory.Cancelled();
        var plan = sub.Plan;
        Assert.Throws<DomainRuleException>(() => sub.ChangePlan(SubscriptionFactory.Plan(1, 14, 10.90m)));
        Assert.Equal(plan, sub.Plan);
    }

    [Fact]
    public void A_paused_subscription_cannot_skip_a_delivery()
    {
        var sub = SubscriptionFactory.Active();
        sub.Pause(SubscriptionFactory.Now);
        var delivery = SubscriptionFactory.NextScheduled(sub);
        Assert.Throws<DomainRuleException>(() => sub.SkipNextDelivery(SubscriptionFactory.Now));
        Assert.Equal(DeliveryStatus.Scheduled, delivery.Status);
        Assert.Equal(2, sub.Deliveries.Count);
    }

    [Fact]
    public void A_due_delivery_can_be_skipped_before_an_order_is_created()
    {
        var sub = SubscriptionFactory.Active();
        var delivery = SubscriptionFactory.NextScheduled(sub);
        sub.SkipNextDelivery(delivery.ScheduledFor.AddDays(2));
        Assert.Equal(DeliveryStatus.Skipped, delivery.Status);
        Assert.Equal(delivery.ScheduledFor.AddDays(sub.Plan.IntervalDays), sub.NextDeliveryAt);
    }
}
