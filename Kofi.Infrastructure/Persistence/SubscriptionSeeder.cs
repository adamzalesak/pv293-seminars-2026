using Kofi.Domain.Customers;
using Kofi.Domain.Orders;
using Kofi.Domain.Products;
using Kofi.Domain.Shared;
using Kofi.Domain.Subscriptions;

namespace Kofi.Infrastructure.Persistence;

public static class SubscriptionSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.Subscriptions.Any())
        {
            return;
        }

        var coffee = db.Products.Single(p => p.Name == "Etiopie Yirgacheffe 250 g");
        var filters = db.Products.Single(p => p.Name == "Papírové filtry V60 02, 100 ks");
        var now = DateTime.Now;
        var petra = db.Customers.Single(c => c.Email.Value == "petra@example.com");
        var jan = db.Customers.Single(c => c.Email.Value == "jan@example.com");
        var eva = db.Customers.Single(c => c.Email.Value == "eva@example.com");
        var miriam = Customer.Register(Guid.NewGuid(), "Miriam Veselá", new Email("miriam@example.com"),
            "Jaselská 8", "Brno", "60200", now.AddDays(-2), paymentMethodToken: "tok_visa", paymentCardLabel: "Visa ending 4242");
        db.Customers.Add(miriam);

        // The seed goes through the same operations the application uses, so
        // it cannot put a subscription into a state the rules forbid.
        // Petra has a delivery due today; Jan is paused and Eva is cancelled.
        db.Subscriptions.Add(Running(db, coffee, petra, now.AddDays(-42), now.AddDays(-21)));

        var paused = Running(db, coffee, jan, now.AddDays(-70));
        paused.Pause(now.AddDays(-2));
        var waiting = paused.Deliveries.Single(d => d.Status == DeliveryStatus.Scheduled);
        paused.AddExtra(waiting.Id, new OfferedProduct(filters.Id, filters.Name, filters.Price, filters.IsActive), new Quantity(1));
        db.Subscriptions.Add(paused);

        var cancelled = Running(db, coffee, eva, now.AddDays(-70));
        cancelled.Cancel(now.AddDays(-10));
        db.Subscriptions.Add(cancelled);

        db.SaveChanges();
    }

    private static Subscription Running(
        AppDbContext db, Product coffee, Customer customer, params DateTime[] orderedAt)
    {
        var subscription = Subscription.Create(
            customer.Id,
            new OfferedProduct(coffee.Id, coffee.Name, coffee.Price, coffee.IsActive), new Quantity(2), intervalDays: 21,
            firstDeliveryOn: orderedAt[0],
            now: orderedAt[0].AddDays(-1));
        subscription.Activate(orderedAt[0]);

        foreach (var day in orderedAt)
        {
            var order = ShippedOrder(db, coffee, customer, day);
            var due = subscription.Deliveries.Single(d => d.Status == DeliveryStatus.Scheduled);
            subscription.PrepareDelivery(due.Id, order.Id, day);
        }

        return subscription;
    }

    private static Order ShippedOrder(
        AppDbContext db, Product coffee, Customer customer, DateTime orderedAt)
    {
        var order = Order.Create(Guid.NewGuid(), customer.Id, customer.Name, customer.Email,
            customer.Street, customer.City, customer.Zip, orderedAt,
            [OrderItem.Create(coffee.Id, coffee.Name, new Quantity(2), coffee.Price)]);
        order.RecordPayment(Guid.NewGuid());
        order.Ship();
        db.Orders.Add(order);

        return order;
    }
}
