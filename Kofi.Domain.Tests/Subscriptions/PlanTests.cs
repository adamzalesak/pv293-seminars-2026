using Kofi.Domain.Shared;
using Kofi.Domain.Subscriptions;
using Xunit;

namespace Kofi.Domain.Tests.Subscriptions;

// The plan is a value: it has no identity, it checks itself, and a change
// replaces the whole of it.
public class PlanTests
{
    [Fact]
    public void A_plan_needs_an_interval()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => SubscriptionFactory.Plan(intervalDays: 0));
    }

    [Fact]
    public void The_line_total_is_the_price_times_the_quantity()
    {
        var plan = SubscriptionFactory.Plan(quantity: 3);

        Assert.Equal(new Price(37.50m, "EUR"), plan.LineTotal);
    }

    [Fact]
    public void Two_plans_with_the_same_terms_are_the_same_plan()
    {
        var plan = SubscriptionFactory.Plan();
        var same = SubscriptionFactory.Plan();

        Assert.Equal(plan, same);
    }
}
