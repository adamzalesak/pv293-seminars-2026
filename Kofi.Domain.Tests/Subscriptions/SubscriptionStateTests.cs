using Kofi.Domain.Subscriptions;
using Kofi.Domain;
using Xunit;

namespace Kofi.Domain.Tests.Subscriptions;

// The state machine, stated once. Every transition the business allows, and
// every one it does not.
public class SubscriptionStateTests
{
    [Fact]
    public void An_incomplete_subscription_becomes_active()
    {
        var subscription = SubscriptionFactory.Incomplete();

        subscription.Activate(SubscriptionFactory.Now);

        Assert.Equal(SubscriptionStatus.Active, subscription.Status);
        Assert.Equal(SubscriptionFactory.Now, subscription.ActivatedAt);
    }

    [Fact]
    public void An_active_subscription_cannot_be_activated_again()
    {
        var subscription = SubscriptionFactory.Active();

        Assert.Throws<DomainRuleException>(
            () => subscription.Activate(SubscriptionFactory.Now));

        Assert.Equal(2, subscription.Deliveries.Count);
    }

    [Fact]
    public void An_active_subscription_can_be_paused()
    {
        var subscription = SubscriptionFactory.Active();

        subscription.Pause(SubscriptionFactory.Now);

        Assert.Equal(SubscriptionStatus.Paused, subscription.Status);
        Assert.Equal(SubscriptionFactory.Now, subscription.PausedAt);
    }

    [Fact]
    public void An_incomplete_subscription_cannot_be_paused()
    {
        var subscription = SubscriptionFactory.Incomplete();

        Assert.Throws<DomainRuleException>(
            () => subscription.Pause(SubscriptionFactory.Now));

        Assert.Equal(SubscriptionStatus.Incomplete, subscription.Status);
    }

    [Fact]
    public void A_cancelled_subscription_cannot_be_paused()
    {
        var subscription = SubscriptionFactory.Cancelled();

        Assert.Throws<DomainRuleException>(
            () => subscription.Pause(SubscriptionFactory.Later().AddDays(1)));

        Assert.Equal(SubscriptionStatus.Cancelled, subscription.Status);
        Assert.Null(subscription.PausedAt);
    }

    [Fact]
    public void A_paused_subscription_resumes_and_gets_a_new_delivery_date()
    {
        var subscription = SubscriptionFactory.Active();
        subscription.Pause(SubscriptionFactory.Now);
        var nextDay = SubscriptionFactory.Now.Date.AddDays(12);

        subscription.Resume(nextDay, SubscriptionFactory.Now.AddDays(10));

        Assert.Equal(SubscriptionStatus.Active, subscription.Status);
        Assert.Null(subscription.PausedAt);
        Assert.Equal(nextDay, subscription.NextDeliveryAt);
    }

    [Fact]
    public void Resuming_for_today_makes_the_waiting_delivery_due()
    {
        var subscription = SubscriptionFactory.Active();
        subscription.Pause(SubscriptionFactory.Now);
        var resumedAt = SubscriptionFactory.Now.AddDays(3);

        subscription.Resume(resumedAt.Date, resumedAt);

        var waiting = SubscriptionFactory.NextScheduled(subscription);
        subscription.PrepareDelivery(waiting.Id, Guid.NewGuid(), resumedAt);
        Assert.Equal(DeliveryStatus.Ready, waiting.Status);
    }

    [Fact]
    public void A_resumed_delivery_cannot_be_in_the_past()
    {
        var subscription = SubscriptionFactory.Active();
        subscription.Pause(SubscriptionFactory.Now);

        Assert.Throws<DomainRuleException>(
            () => subscription.Resume(SubscriptionFactory.Now.Date.AddDays(-1), SubscriptionFactory.Now));

        Assert.Equal(SubscriptionStatus.Paused, subscription.Status);
    }

    [Fact]
    public void Resuming_moves_the_waiting_delivery_instead_of_adding_one()
    {
        var subscription = SubscriptionFactory.Active();
        subscription.Pause(SubscriptionFactory.Now);
        subscription.Resume(SubscriptionFactory.Now.Date.AddDays(12), SubscriptionFactory.Now.AddDays(10));

        var waiting = SubscriptionFactory.NextScheduled(subscription);
        Assert.Equal(2, subscription.Deliveries.Count);
        Assert.Equal(subscription.NextDeliveryAt, waiting.ScheduledFor);
    }

    [Fact]
    public void An_active_subscription_cannot_be_resumed()
    {
        var subscription = SubscriptionFactory.Active();

        Assert.Throws<DomainRuleException>(
            () => subscription.Resume(SubscriptionFactory.Now.Date, SubscriptionFactory.Now));

        Assert.Equal(SubscriptionStatus.Active, subscription.Status);
    }

    [Fact]
    public void Cancelling_twice_is_rejected()
    {
        var subscription = SubscriptionFactory.Cancelled();
        var cancelledAt = subscription.CancelledAt;

        Assert.Throws<DomainRuleException>(
            () => subscription.Cancel(SubscriptionFactory.Later().AddDays(1)));

        Assert.Equal(cancelledAt, subscription.CancelledAt);
    }

    [Fact]
    public void Activating_records_the_fact_for_whoever_reacts()
    {
        var subscription = SubscriptionFactory.Incomplete();

        subscription.Activate(SubscriptionFactory.Now);

        var activated = Assert.IsType<SubscriptionActivated>(Assert.Single(subscription.DomainEvents));
        Assert.Equal(subscription.Id, activated.SubscriptionId);
    }
}
