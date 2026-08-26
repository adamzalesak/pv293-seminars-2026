using Kofi.DataAccessLayer.Models;

namespace Kofi.DataAccessLayer.Data;

public static class SubscriptionSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.Subscriptions.Any())
            return;

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

        AddHistory(db, coffee, petra, Guid.Parse("40000000-0000-0000-0000-000000000001"),
            SubscriptionStatus.Active, now.AddDays(-42));
        var paused = AddHistory(db, coffee, jan, Guid.Parse("40000000-0000-0000-0000-000000000002"),
            SubscriptionStatus.Paused, now.AddDays(-70));
        AddHistory(db, coffee, eva, Guid.Parse("40000000-0000-0000-0000-000000000003"),
            SubscriptionStatus.Cancelled, now.AddDays(-70));
        var waiting = paused.Deliveries.Single(d => d.Status == DeliveryStatus.Scheduled);
        waiting.Extras.Add(new SubscriptionDeliveryExtra
        {
            DeliveryId = waiting.Id, ProductId = filters.Id, ProductName = filters.Name,
            Quantity = 1, UnitPrice = filters.Price, Currency = "EUR",
        });
        db.SaveChanges();
    }

    private static Subscription AddHistory(AppDbContext db, Product coffee, Customer customer, Guid id,
        SubscriptionStatus status, DateTime orderedAt)
    {
        var now = DateTime.Now;
        var subscription = new Subscription
        {
            Id = id, CustomerId = customer.Id, ProductId = coffee.Id, ProductName = coffee.Name,
            UnitPrice = coffee.Price, Quantity = 2, IntervalDays = 21, Status = status,
            CreatedAt = orderedAt.AddDays(-1), ActivatedAt = orderedAt,
            PausedAt = status == SubscriptionStatus.Paused ? now.AddDays(-2) : null,
            CancelledAt = status == SubscriptionStatus.Cancelled ? now.AddDays(-10) : null,
            NextDeliveryAt = status == SubscriptionStatus.Cancelled ? null :
                status == SubscriptionStatus.Active ? now.Date : now.AddDays(17),
        };
        AddShippedOrder(db, subscription, coffee, customer, orderedAt);
        if (status != SubscriptionStatus.Cancelled)
        {
            // A later delivery has also shipped; the next one is still planned.
            AddShippedOrder(db, subscription, coffee, customer,
                status == SubscriptionStatus.Active ? now.AddDays(-21) : now.AddDays(-6));
            subscription.Deliveries.Add(new SubscriptionDelivery
            {
                Id = Guid.NewGuid(), SubscriptionId = id, ProductId = coffee.Id,
                Quantity = 2, UnitPrice = coffee.Price, ScheduledFor = subscription.NextDeliveryAt!.Value,
            });
        }
        db.Subscriptions.Add(subscription);
        return subscription;
    }

    private static void AddShippedOrder(AppDbContext db, Subscription subscription, Product coffee,
        Customer customer, DateTime orderedAt)
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
        subscription.Deliveries.Add(new SubscriptionDelivery
        {
            Id = Guid.NewGuid(), SubscriptionId = subscription.Id, ProductId = coffee.Id,
            Quantity = 2, UnitPrice = coffee.Price, ScheduledFor = orderedAt,
            Status = DeliveryStatus.Ready, OrderId = order.Id,
        });
    }
}
