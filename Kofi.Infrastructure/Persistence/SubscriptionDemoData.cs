using Kofi.Domain.Shared;
using Kofi.Domain.Subscriptions;

namespace Kofi.Infrastructure.Persistence;

internal static class SubscriptionDemoData
{
    public static void Seed(AppDbContext db)
    {
        if (db.Subscriptions.Any())
        {
            return;
        }

        var now = DateTime.Now;
        var petra = Running(Guid.Parse("6f1c2a4e-8d3b-4c7a-9e21-5b0f3d8a7c14"), (Guid.Parse("8fbddfd0-75bd-42ab-8bf9-000000000001"), 42),
            (Guid.Parse("8fbddfd0-75bd-42ab-8bf9-000000000002"), 21));
        var jan = Running(Guid.Parse("4d8b2f61-9a3e-4c7d-b5e0-1f6a8c2d9e34"), (Guid.Parse("8fbddfd0-75bd-42ab-8bf9-000000000003"), 70));
        jan.Pause(now.AddDays(-2));
        jan.AddExtra(jan.Deliveries.Single(d => d.Status == DeliveryStatus.Scheduled).Id,
            new OfferedProduct(Guid.Parse("6b0e3f84-2d7a-4c19-9e56-b1a8d4c7f2e0"), "Papírové filtry V60 02, 100 ks",
                new Price(6.90m, "EUR"), true), new Quantity(1));
        var eva = Running(Guid.Parse("a7e3c9d1-2b5f-4e8a-9c06-3d1f7b4e8a45"), (Guid.Parse("8fbddfd0-75bd-42ab-8bf9-000000000004"), 70));
        eva.Cancel(now.AddDays(-10));
        db.Subscriptions.AddRange(petra, jan, eva);
        db.SaveChanges();
    }

    private static Subscription Running(Guid customerId, params (Guid OrderId, int DaysAgo)[] orders)
    {
        var now = DateTime.Now;
        var first = now.AddDays(-orders[0].DaysAgo);
        var subscription = Subscription.Create(customerId,
            new OfferedProduct(Guid.Parse("3f2a8c61-5d4e-4b7a-9e1c-0a6b2d8f4e11"), "Etiopie Yirgacheffe 250 g", new Price(12.50m, "EUR"),
                true),
            new Quantity(2), intervalDays: 21, firstDeliveryOn: first, now: first.AddDays(-1));
        subscription.Activate(first);

        foreach (var order in orders)
        {
            subscription.PrepareDelivery(subscription.Deliveries.Single(d => d.Status == DeliveryStatus.Scheduled).Id,
                order.OrderId, now.AddDays(-order.DaysAgo));
        }

        return subscription;
    }
}
