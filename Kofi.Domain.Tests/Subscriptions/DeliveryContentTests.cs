using Kofi.Domain;
using Kofi.Domain.Shared;
using Kofi.Domain.Subscriptions;
using Xunit;

namespace Kofi.Domain.Tests.Subscriptions;

public class DeliveryContentTests
{
    private static readonly Guid Filters = Guid.Parse("f1a0c2b4-0000-0000-0000-000000000003");

    private static void AddFilters(Subscription subscription, Delivery delivery, int quantity = 1, decimal price = 6.90m) =>
        subscription.AddExtra(delivery.Id, Filters, "V60 filters", quantity, new Price(price, "EUR"));

    [Fact]
    public void Repeated_additions_keep_the_first_agreed_price()
    {
        var sub = SubscriptionFactory.Active();
        var delivery = SubscriptionFactory.NextScheduled(sub);
        AddFilters(sub, delivery);
        AddFilters(sub, delivery, 2, 9.90m);
        var extra = Assert.Single(delivery.Extras);
        Assert.Equal(3, extra.Quantity);
        Assert.Equal(new Price(6.90m, "EUR"), extra.UnitPrice);
    }

    [Fact]
    public void An_invalid_addition_leaves_the_quantity_unchanged()
    {
        var sub = SubscriptionFactory.Active();
        var delivery = SubscriptionFactory.NextScheduled(sub);
        AddFilters(sub, delivery, 100);
        Assert.Throws<DomainRuleException>(() => AddFilters(sub, delivery));
        Assert.Equal(100, Assert.Single(delivery.Extras).Quantity);
    }

    [Theory]
    [InlineData(0, "EUR")]
    [InlineData(101, "EUR")]
    [InlineData(1, "USD")]
    public void Extras_require_a_valid_quantity_and_the_delivery_currency(int quantity, string currency)
    {
        var sub = SubscriptionFactory.Active();
        var delivery = SubscriptionFactory.NextScheduled(sub);
        Assert.Throws<DomainRuleException>(() => sub.AddExtra(delivery.Id, Filters, "Filters", quantity, new Price(6.90m, currency)));
        Assert.Empty(delivery.Extras);
    }

    [Fact]
    public void Skipping_discards_extras_from_the_next_delivery_and_preserves_history()
    {
        var sub = SubscriptionFactory.Active();
        var skipped = SubscriptionFactory.NextScheduled(sub);
        AddFilters(sub, skipped);
        var oldDate = skipped.ScheduledFor;
        var oldPrice = skipped.UnitPrice;
        sub.SkipNextDelivery(SubscriptionFactory.Now);
        sub.ChangePlan(SubscriptionFactory.Plan(1, 14, 10.90m));
        Assert.Equal(DeliveryStatus.Skipped, skipped.Status);
        Assert.Equal(oldDate, skipped.ScheduledFor);
        Assert.Equal(oldPrice, skipped.UnitPrice);
        Assert.Single(skipped.Extras);
        Assert.Empty(SubscriptionFactory.NextScheduled(sub).Extras);
    }

    [Fact]
    public void Pausing_and_resuming_preserves_the_delivery_and_its_extras()
    {
        var sub = SubscriptionFactory.Active();
        var delivery = SubscriptionFactory.NextScheduled(sub);
        AddFilters(sub, delivery);
        sub.Pause(SubscriptionFactory.Now);
        AddFilters(sub, delivery);
        var nextDay = SubscriptionFactory.Now.Date.AddDays(45);
        sub.Resume(nextDay, SubscriptionFactory.Now.AddDays(40));
        Assert.Same(delivery, SubscriptionFactory.NextScheduled(sub));
        Assert.Equal(nextDay, delivery.ScheduledFor);
        Assert.Equal(2, Assert.Single(delivery.Extras).Quantity);
    }

    [Fact]
    public void Changing_the_plan_keeps_the_next_delivery_date()
    {
        var sub = SubscriptionFactory.Active();
        var delivery = SubscriptionFactory.NextScheduled(sub);
        var date = delivery.ScheduledFor;
        sub.ChangePlan(SubscriptionFactory.Plan(1, 28, 10.90m));
        Assert.Equal(date, delivery.ScheduledFor);
        Assert.Equal(date, sub.NextDeliveryAt);
        sub.PrepareDelivery(delivery.Id, Guid.NewGuid(), date);
        Assert.Equal(date.AddDays(28), SubscriptionFactory.NextScheduled(sub).ScheduledFor);
    }

    [Fact]
    public void Changing_the_plan_preserves_one_time_extras()
    {
        var sub = SubscriptionFactory.Active();
        var delivery = SubscriptionFactory.NextScheduled(sub);
        AddFilters(sub, delivery);
        sub.ChangePlan(SubscriptionFactory.Plan(1, 14, 10.90m));
        Assert.Equal(1, delivery.Quantity);
        Assert.Equal(new Price(10.90m, "EUR"), delivery.UnitPrice);
        Assert.Equal(new Price(6.90m, "EUR"), Assert.Single(delivery.Extras).UnitPrice);
    }

    [Fact]
    public void Preparing_a_late_delivery_keeps_the_original_calendar_and_starts_without_extras()
    {
        var sub = SubscriptionFactory.Active();
        var delivery = SubscriptionFactory.NextScheduled(sub);
        AddFilters(sub, delivery);
        var orderId = Guid.NewGuid();
        sub.PrepareDelivery(delivery.Id, orderId, delivery.ScheduledFor.AddDays(5));
        Assert.Equal(DeliveryStatus.Ready, delivery.Status);
        Assert.Equal(orderId, delivery.OrderId);
        Assert.Single(delivery.Extras);
        var next = SubscriptionFactory.NextScheduled(sub);
        Assert.Equal(delivery.ScheduledFor.AddDays(21), next.ScheduledFor);
        Assert.Empty(next.Extras);
    }

    [Fact]
    public void Preparing_requires_a_due_active_delivery_and_cannot_be_repeated()
    {
        var sub = SubscriptionFactory.Active();
        var delivery = SubscriptionFactory.NextScheduled(sub);
        Assert.Throws<DomainRuleException>(() => sub.PrepareDelivery(delivery.Id, Guid.NewGuid(), SubscriptionFactory.Now));
        sub.Pause(SubscriptionFactory.Now);
        Assert.Throws<DomainRuleException>(() => sub.PrepareDelivery(delivery.Id, Guid.NewGuid(), delivery.ScheduledFor));
        sub.Resume(delivery.ScheduledFor, SubscriptionFactory.Now);
        sub.PrepareDelivery(delivery.Id, Guid.NewGuid(), delivery.ScheduledFor);
        var count = sub.Deliveries.Count;
        Assert.Throws<DomainRuleException>(() => sub.PrepareDelivery(delivery.Id, Guid.NewGuid(), delivery.ScheduledFor));
        Assert.Equal(count, sub.Deliveries.Count);
    }

    [Fact]
    public void History_and_deliveries_of_other_subscriptions_cannot_receive_extras()
    {
        var sub = SubscriptionFactory.Active();
        var delivery = SubscriptionFactory.NextScheduled(sub);
        Assert.Throws<DomainRuleException>(() => AddFilters(sub, SubscriptionFactory.FirstDelivery(sub)));
        Assert.Throws<DomainRuleException>(() => sub.AddExtra(Guid.NewGuid(), Filters, "Filters", 1, new Price(6.90m, "EUR")));
        sub.SkipNextDelivery(SubscriptionFactory.Now);
        Assert.Throws<DomainRuleException>(() => AddFilters(sub, delivery));
        var cancelled = SubscriptionFactory.NextScheduled(sub);
        sub.Cancel(SubscriptionFactory.Now);
        Assert.Throws<DomainRuleException>(() => AddFilters(sub, cancelled));
    }

    [Fact]
    public void Removing_an_extra_changes_only_its_owned_planned_delivery()
    {
        var sub = SubscriptionFactory.Active();
        var delivery = SubscriptionFactory.NextScheduled(sub);
        AddFilters(sub, delivery);
        sub.RemoveExtra(delivery.Id, Filters);
        Assert.Empty(delivery.Extras);
        Assert.Throws<DomainRuleException>(() => sub.RemoveExtra(delivery.Id, Filters));
    }

    [Fact]
    public void Extras_cannot_be_mutated_through_the_public_collection()
    {
        var sub = SubscriptionFactory.Active();
        var delivery = SubscriptionFactory.NextScheduled(sub);
        AddFilters(sub, delivery);
        var extras = (ICollection<DeliveryExtra>)delivery.Extras;
        Assert.Throws<NotSupportedException>(() => extras.Clear());
        Assert.Single(delivery.Extras);
    }
}
