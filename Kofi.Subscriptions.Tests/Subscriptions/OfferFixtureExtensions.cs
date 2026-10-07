using Kofi.Subscriptions.Domain.Shared;
using Kofi.Subscriptions.Domain.Subscriptions;

namespace Kofi.Subscriptions.Tests.Subscriptions;

internal static class OfferFixtureExtensions
{
    public static void ChangePlan(this Subscription subscription, Plan plan) => subscription.ChangePlan(
        new OfferedProduct(plan.ProductId, plan.ProductName, plan.UnitPrice, true), plan.Quantity, plan.IntervalDays);

    public static void AddExtra(this Subscription subscription, Guid deliveryId, Guid id, string name, Quantity quantity, Price price) =>
        subscription.AddExtra(deliveryId, new OfferedProduct(id, name, price, true), quantity);
}
