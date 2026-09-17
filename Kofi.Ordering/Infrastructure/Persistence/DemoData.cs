using Kofi.Ordering.Domain.Customers;
using Kofi.Ordering.Domain.Orders;

namespace Kofi.Ordering.Infrastructure.Persistence;

internal static class DemoData
{
    private static readonly Dictionary<string, (Guid Id, decimal Price)> Products = new()
    {
        ["Etiopie Yirgacheffe 250 g"] = (Guid.Parse("3f2a8c61-5d4e-4b7a-9e1c-0a6b2d8f4e11"), 12.50m),
        ["Kolumbie Huila 250 g"] = (Guid.Parse("7c1e9b42-0f3d-4a6e-8b25-d4f1a9c3e522"), 10.90m),
        ["Výběr pražírny — ovocná 250 g"] = (Guid.Parse("d3a7e2c5-8b14-4f69-a0d2-6c5e1b9f4a71"), 11.90m),
        ["Výběr pražírny — čokoládová 250 g"] = (Guid.Parse("6e0b9f37-c2a8-4d15-b7e4-0f3a8d6c2e92"), 10.90m),
        ["Brazílie Santos 1 kg"] = (Guid.Parse("b5d84e17-2a9c-4f3b-a6e0-91c7f2d5b833"), 27.90m),
        ["Etiopie Yirgacheffe mletá 250 g"] = (Guid.Parse("e9a6c3d0-7b1f-4e85-9d42-3f0b8a6c1d44"), 12.90m),
        ["Kapsle Espresso Forte 10 ks"] = (Guid.Parse("1d7f0a95-c4e2-4b68-8a3d-6e9c2f1b7a55"), 4.90m),
        ["Kapsle Lungo Delicato 10 ks"] = (Guid.Parse("5a3e8d26-9f1b-4c7e-b0a4-2d6f8e1c9b66"), 4.90m),
        ["Ruční mlýnek Kompakt"] = (Guid.Parse("8e2b5f19-3c7a-4d0e-91b6-a4f2c8e5d077"), 89.00m),
        ["Papírové filtry V60 02, 100 ks"] = (Guid.Parse("6b0e3f84-2d7a-4c19-9e56-b1a8d4c7f2e0"), 6.90m),
        ["Překapávač V60 02"] = (Guid.Parse("2c9d4a70-e6b3-4f81-8d5c-7b1e3a9f6088"), 24.90m),
        ["Konvice s husím krkem 1 l"] = (Guid.Parse("f4b1c7e8-5a2d-4e93-b7f0-c3e9d6a2b199"), 44.90m),
        ["Sypaný čaj Darjeeling 100 g"] = (Guid.Parse("9b6e2d53-1f8c-4a07-a5e3-8d4b0c7f2e1a"), 8.50m),
        ["Cupping lžíce"] = (Guid.Parse("c8f3a1b4-6d9e-4c25-8e71-5b2a0f9d3c2b"), 9.90m)
    };

    public static void Seed(OrderingDbContext ordering)
    {
        if (ordering.Customers.Any())
        {
            return;
        }

        ordering.Customers.AddRange(
            Customer.Register(Guid.Parse("6f1c2a4e-8d3b-4c7a-9e21-5b0f3d8a7c14"), "Petra Nováková", new Email("petra@example.com"),
                new ShippingAddress("Kounicova 12", "Brno", "60200"), DateTime.Now.AddMonths(-3), isVip: true,
                paymentMethodToken: "tok_visa", paymentCardLabel: "Visa ending 4242"),
            Customer.Register(Guid.Parse("4d8b2f61-9a3e-4c7d-b5e0-1f6a8c2d9e34"), "Jan Dvořák", new Email("jan@example.com"),
                new ShippingAddress("Veveří 45", "Brno", "60200"), DateTime.Now.AddMonths(-2), paymentMethodToken: "tok_visa",
                paymentCardLabel: "Visa ending 4242"),
            Customer.Register(Guid.Parse("a7e3c9d1-2b5f-4e8a-9c06-3d1f7b4e8a45"), "Eva Horáková", new Email("eva@example.com"),
                new ShippingAddress("Palackého 3", "Praha", "11000"), DateTime.Now.AddDays(-20), paymentMethodToken: "tok_visa",
                paymentCardLabel: "Visa ending 4242")
        );

        ordering.Customers.Add(Customer.Register(Guid.Parse("f090d75e-c2eb-49de-8cc4-8a841968e079"), "Miriam Veselá",
            new Email("miriam@example.com"),
            new ShippingAddress("Jaselská 8", "Brno", "60200"), DateTime.Now.AddDays(-2), paymentMethodToken: "tok_visa",
            paymentCardLabel: "Visa ending 4242"));
        ordering.SaveChanges();

        // A few months of orders in every state.
        var petra = ordering.Customers.Find(Guid.Parse("6f1c2a4e-8d3b-4c7a-9e21-5b0f3d8a7c14"))!;
        var jan = ordering.Customers.Find(Guid.Parse("4d8b2f61-9a3e-4c7d-b5e0-1f6a8c2d9e34"))!;
        var eva = ordering.Customers.Find(Guid.Parse("a7e3c9d1-2b5f-4e8a-9c06-3d1f7b4e8a45"))!;
        ordering.Orders.AddRange(
            SeedOrder(Guid.NewGuid(), petra, OrderStatus.Shipped, daysAgo: 60, ("Etiopie Yirgacheffe 250 g", 2), ("Překapávač V60 02", 1)),
            SeedOrder(Guid.NewGuid(), jan, OrderStatus.Shipped, daysAgo: 41, ("Brazílie Santos 1 kg", 1)),
            SeedOrder(Guid.NewGuid(), jan, OrderStatus.Cancelled, daysAgo: 30, ("Ruční mlýnek Kompakt", 1)),
            SeedOrder(Guid.NewGuid(), eva, OrderStatus.Paid, daysAgo: 2, ("Kolumbie Huila 250 g", 1), ("Kapsle Lungo Delicato 10 ks", 2)),
            SeedOrder(Guid.NewGuid(), petra, OrderStatus.Paid, daysAgo: 1, ("Konvice s husím krkem 1 l", 1), ("Kolumbie Huila 250 g", 2)),
            SeedOrder(Guid.NewGuid(), petra, OrderStatus.New, daysAgo: 0, ("Kapsle Espresso Forte 10 ks", 3))
        );

        ordering.Orders.AddRange(
            SeedOrder(Guid.Parse("8fbddfd0-75bd-42ab-8bf9-000000000001"), petra, OrderStatus.Shipped, 42, ("Etiopie Yirgacheffe 250 g", 2)),
            SeedOrder(Guid.Parse("8fbddfd0-75bd-42ab-8bf9-000000000002"), petra, OrderStatus.Shipped, 21, ("Etiopie Yirgacheffe 250 g", 2)),
            SeedOrder(Guid.Parse("8fbddfd0-75bd-42ab-8bf9-000000000003"), jan, OrderStatus.Shipped, 70, ("Etiopie Yirgacheffe 250 g", 2)),
            SeedOrder(Guid.Parse("8fbddfd0-75bd-42ab-8bf9-000000000004"), eva, OrderStatus.Shipped, 70, ("Etiopie Yirgacheffe 250 g", 2)));
        ordering.SaveChanges();
    }

    private static Order SeedOrder(Guid id, Customer customer, OrderStatus status, int daysAgo,
        params (string Product, int Quantity)[] lines)
    {
        var items = lines.Select(line =>
        {
            var product = Products[line.Product];
            return OrderItem.Create(product.Id, line.Product, new Quantity(line.Quantity), new Price(product.Price, "EUR"));
        });
        var order = Order.Create(id, customer.Id, customer.Name, customer.Email,
            customer.ShippingAddress, DateTime.Now.AddDays(-daysAgo).AddHours(-3), items);

        if (status is OrderStatus.Paid or OrderStatus.Shipped)
        {
            order.RecordPayment(Guid.NewGuid());
        }

        if (status == OrderStatus.Shipped)
        {
            order.Ship();
        }

        if (status == OrderStatus.Cancelled)
        {
            order.Cancel();
        }

        return order;
    }
}
