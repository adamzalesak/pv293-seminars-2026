using Kofi.DataAccessLayer.Models;
using Kofi.Domain.Shared;
using Kofi.Domain.Subscriptions;

namespace Kofi.DataAccessLayer.Data;

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
        var petra = db.Customers.Single(c => c.Email == "petra@example.com");
        var jan = db.Customers.Single(c => c.Email == "jan@example.com");
        var eva = db.Customers.Single(c => c.Email == "eva@example.com");
        var miriam = new Customer
        {
            Id = Guid.NewGuid(), Name = "Miriam Veselá", Email = "miriam@example.com",
            Street = "Jaselská 8", City = "Brno", Zip = "60200", RegisteredAt = now.AddDays(-2),
            PaymentMethodToken = "tok_visa", PaymentCardLabel = "Visa ending 4242",
        };
        db.Customers.Add(miriam);

        // The seed goes through the same operations the application uses, so
        // it cannot put a subscription into a state the rules forbid.
        // Petra has a delivery due today; Jan is paused and Eva is cancelled.
        db.Subscriptions.Add(Running(db, coffee, petra, now.AddDays(-42), now.AddDays(-21)));

        var paused = Running(db, coffee, jan, now.AddDays(-70));
        paused.Pause(now.AddDays(-2));
        var waiting = paused.Deliveries.Single(d => d.Status == DeliveryStatus.Scheduled);
        paused.AddExtra(waiting.Id, filters.Id, filters.Name, new Quantity(1), new Price(filters.Price, "EUR"));
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
            new Plan(coffee.Id, coffee.Name, new Quantity(2), intervalDays: 21, new Price(coffee.Price, "EUR")),
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
        var order = new Order
        {
            Id = Guid.NewGuid(), CustomerId = customer.Id, CustomerName = customer.Name,
            CustomerEmail = customer.Email, Street = customer.Street, City = customer.City,
            Zip = customer.Zip, Status = OrderStatus.Shipped, CreatedAt = orderedAt,
        };
        order.Items.Add(new OrderItem
        {
            Id = Guid.NewGuid(), OrderId = order.Id, ProductId = coffee.Id,
            ProductName = coffee.Name, UnitPrice = coffee.Price, Quantity = 2,
        });
        db.Orders.Add(order);
        return order;
    }
}
