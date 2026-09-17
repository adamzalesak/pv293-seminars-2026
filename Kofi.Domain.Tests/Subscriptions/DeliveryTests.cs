using Kofi.Common.Domain;
using Kofi.Domain.Shared;
using Kofi.Domain.Subscriptions;
using Xunit;

namespace Kofi.Domain.Tests.Subscriptions;

// Deliveries belong to the subscription. Every change to one goes through
// the root, which is what keeps the schedule and the plan consistent.
public class DeliveryTests
{
    [Fact]
    public void Activating_plans_the_first_delivery_for_the_chosen_day()
    {
        var firstDay = SubscriptionFactory.Now.Date.AddDays(5);
        var subscription = SubscriptionFactory.Incomplete(firstDay);

        subscription.Activate(SubscriptionFactory.Now);

        var first = SubscriptionFactory.NextScheduled(subscription);
        Assert.Single(subscription.Deliveries);
        Assert.Equal(firstDay, first.ScheduledFor);
        Assert.Equal(firstDay, subscription.NextDeliveryAt);
        Assert.Throws<DomainRuleException>(() => subscription.PrepareDelivery(first.Id, Guid.NewGuid(), SubscriptionFactory.Now));
    }

    [Fact]
    public void A_first_delivery_due_today_is_prepared_and_the_next_one_scheduled()
    {
        var subscription = SubscriptionFactory.Incomplete();
        subscription.Activate(SubscriptionFactory.Now);
        var first = subscription.Deliveries.Single();

        subscription.PrepareDelivery(first.Id, SubscriptionFactory.FirstOrderId, SubscriptionFactory.Now);

        var next = SubscriptionFactory.NextScheduled(subscription);
        Assert.Equal(DeliveryStatus.Ready, first.Status);
        Assert.Equal(SubscriptionFactory.Now.Date, first.ScheduledFor);
        Assert.Equal(SubscriptionFactory.Now.Date.AddDays(21), next.ScheduledFor);
        Assert.Equal(next.ScheduledFor, subscription.NextDeliveryAt);
    }

    [Fact]
    public void A_delivery_due_now_is_prepared_with_one_line_per_product()
    {
        var subscription = SubscriptionFactory.Incomplete();
        subscription.Activate(SubscriptionFactory.Now);

        var prepared = subscription.PrepareIfDue(SubscriptionFactory.FirstOrderId, SubscriptionFactory.Now);

        Assert.NotNull(prepared);
        Assert.Equal(SubscriptionFactory.FirstOrderId, prepared.OrderId);
        var line = Assert.Single(prepared.Lines);
        Assert.Equal(SubscriptionFactory.ProductId, line.ProductId);
        Assert.Equal(new Quantity(2), line.Quantity);
        Assert.Equal(new Price(12.50m, "EUR"), line.UnitPrice);
    }

    [Fact]
    public void A_delivery_planned_for_later_is_not_prepared_yet()
    {
        var subscription = SubscriptionFactory.Incomplete(SubscriptionFactory.Now.Date.AddDays(5));
        subscription.Activate(SubscriptionFactory.Now);

        Assert.Null(subscription.PrepareIfDue(Guid.NewGuid(), SubscriptionFactory.Now));
        Assert.Equal(DeliveryStatus.Scheduled, subscription.Deliveries.Single().Status);
    }

    [Fact]
    public void A_first_day_that_passed_before_activation_becomes_today()
    {
        var subscription = SubscriptionFactory.Incomplete(SubscriptionFactory.Now.Date.AddDays(2));
        var activatedAt = SubscriptionFactory.Now.AddDays(5);

        subscription.Activate(activatedAt);

        Assert.Equal(activatedAt.Date, SubscriptionFactory.NextScheduled(subscription).ScheduledFor);
    }

    [Fact]
    public void The_first_delivery_cannot_be_in_the_past()
    {
        Assert.Throws<DomainRuleException>(() => SubscriptionFactory.Incomplete(SubscriptionFactory.Now.Date.AddDays(-1)));
    }

    [Fact]
    public void A_delivery_keeps_the_price_it_was_planned_with()
    {
        var subscription = SubscriptionFactory.Active();

        var first = SubscriptionFactory.FirstDelivery(subscription);

        Assert.Equal(new Price(12.50m, "EUR"), first.UnitPrice);
        Assert.Equal(new Quantity(2), first.Quantity);
    }

    [Fact]
    public void Skipping_moves_the_schedule_one_interval_on()
    {
        var subscription = SubscriptionFactory.Active();
        var skipped = SubscriptionFactory.NextScheduled(subscription);
        var expected = skipped.ScheduledFor.AddDays(subscription.Plan.IntervalDays);

        subscription.SkipNextDelivery(SubscriptionFactory.Now);

        Assert.Equal(DeliveryStatus.Skipped, skipped.Status);
        Assert.Equal(expected, subscription.NextDeliveryAt);
        Assert.Contains(subscription.Deliveries,
            d => d.Status == DeliveryStatus.Scheduled && d.ScheduledFor == expected);
    }

    [Fact]
    public void A_due_delivery_can_be_skipped_until_an_order_exists()
    {
        var subscription = SubscriptionFactory.Active();
        var due = SubscriptionFactory.NextScheduled(subscription).ScheduledFor;

        var delivery = SubscriptionFactory.NextScheduled(subscription);
        subscription.SkipNextDelivery(due);

        Assert.Equal(DeliveryStatus.Skipped, delivery.Status);
        Assert.Equal(due.AddDays(subscription.Plan.IntervalDays), subscription.NextDeliveryAt);
    }

    [Fact]
    public void A_paused_subscription_cannot_skip_a_delivery()
    {
        var subscription = SubscriptionFactory.Active();
        subscription.Pause(SubscriptionFactory.Now);

        Assert.Throws<DomainRuleException>(() => subscription.SkipNextDelivery(SubscriptionFactory.Now));
    }

    [Fact]
    public void Changing_the_plan_reprices_what_is_ahead_but_keeps_its_date()
    {
        var subscription = SubscriptionFactory.Active();
        var ahead = SubscriptionFactory.NextScheduled(subscription);
        var date = ahead.ScheduledFor;
        var plan = SubscriptionFactory.Plan(quantity: 1, intervalDays: 14, unitPrice: 10.90m);

        subscription.ChangePlan(plan);

        Assert.Equal(plan, subscription.Plan);
        Assert.Equal(new Price(10.90m, "EUR"), ahead.UnitPrice);
        Assert.Equal(date, ahead.ScheduledFor);
        Assert.Equal(date, subscription.NextDeliveryAt);
    }

    [Fact]
    public void A_changed_interval_counts_from_the_next_delivery()
    {
        var subscription = SubscriptionFactory.Active();
        var ahead = SubscriptionFactory.NextScheduled(subscription);
        subscription.ChangePlan(SubscriptionFactory.Plan(intervalDays: 28));

        subscription.PrepareDelivery(ahead.Id, Guid.NewGuid(), ahead.ScheduledFor);

        Assert.Equal(ahead.ScheduledFor.AddDays(28), SubscriptionFactory.NextScheduled(subscription).ScheduledFor);
    }

    [Fact]
    public void Changing_the_plan_leaves_a_delivered_delivery_at_its_old_price()
    {
        var subscription = SubscriptionFactory.Active();

        subscription.ChangePlan(
            SubscriptionFactory.Plan(quantity: 1, intervalDays: 14, unitPrice: 10.90m));

        Assert.Equal(new Price(12.50m, "EUR"), SubscriptionFactory.FirstDelivery(subscription).UnitPrice);
    }

    [Fact]
    public void Cancelling_cancels_what_was_only_planned()
    {
        var subscription = SubscriptionFactory.Active();

        subscription.Cancel(SubscriptionFactory.Now);

        Assert.DoesNotContain(subscription.Deliveries, d => d.Status == DeliveryStatus.Scheduled);
        Assert.Null(subscription.NextDeliveryAt);
    }

    [Fact]
    public void Cancelling_leaves_a_delivery_that_is_already_on_its_way()
    {
        var subscription = SubscriptionFactory.Active();
        var onItsWay = SubscriptionFactory.FirstDelivery(subscription);

        subscription.Cancel(SubscriptionFactory.Now);

        Assert.Equal(DeliveryStatus.Ready, onItsWay.Status);
    }

    [Fact]
    public void Nothing_outside_the_subscription_can_add_a_delivery()
    {
        var subscription = SubscriptionFactory.Active();

        // The read-only wrapper still looks like a collection, so a caller
        // can reach for Add — and is refused instead of changing the
        // subscription behind its own back.
        var collection = (ICollection<Delivery>)subscription.Deliveries;

        Assert.Throws<NotSupportedException>(() => collection.Add(null!));
        Assert.Equal(2, subscription.Deliveries.Count);
    }

    // NextDeliveryAt is what the API shows; it must point at the delivery
    // that is actually waiting, whatever happened before.
    [Fact]
    public void The_next_delivery_date_always_matches_the_waiting_delivery()
    {
        var firstDay = SubscriptionFactory.Now.Date.AddDays(5);
        var subscription = SubscriptionFactory.Incomplete(firstDay);
        Assert.Equal(firstDay, subscription.NextDeliveryAt);

        subscription.Activate(SubscriptionFactory.Now);
        AssertNextDeliveryMatches(subscription);

        subscription.SkipNextDelivery(SubscriptionFactory.Now);
        AssertNextDeliveryMatches(subscription);

        subscription.Pause(SubscriptionFactory.Now);
        subscription.Resume(SubscriptionFactory.Now.Date.AddDays(3), SubscriptionFactory.Now);
        AssertNextDeliveryMatches(subscription);

        var due = SubscriptionFactory.NextScheduled(subscription);
        subscription.PrepareDelivery(due.Id, Guid.NewGuid(), due.ScheduledFor);
        AssertNextDeliveryMatches(subscription);

        subscription.ChangePlan(SubscriptionFactory.Plan(intervalDays: 14));
        AssertNextDeliveryMatches(subscription);
    }

    private static void AssertNextDeliveryMatches(Subscription subscription) =>
        Assert.Equal(SubscriptionFactory.NextScheduled(subscription).ScheduledFor, subscription.NextDeliveryAt);
}
