using Kofi.Domain.Customers;
using Kofi.Domain.Orders;
using Kofi.Domain.Products;
using Kofi.Domain.Shared;

namespace Kofi.Infrastructure.Persistence;

public static class DbSeeder
{
    // Creates the schema and seeds the demo data on startup.
    public static void Seed(AppDbContext db)
    {
        db.Database.EnsureCreated();

        if (db.Products.Any())
        {
            return;
        }

        // Fixed ids, so the requests in kofi.http keep working after a restart.
        db.Products.AddRange(
            Product.Create(Guid.Parse("3f2a8c61-5d4e-4b7a-9e1c-0a6b2d8f4e11"), "Etiopie Yirgacheffe 250 g",
                "Zrnková, světlé pražení, praná metoda, tóny citrusů", new Price(12.50m, "EUR"), 120, roastedOn: Roasted(6),
                origin: "Etiopie", imageUrl: "/images/products/etiopie-yirgacheffe.jpg"),
            Product.Create(Guid.Parse("7c1e9b42-0f3d-4a6e-8b25-d4f1a9c3e522"), "Kolumbie Huila 250 g",
                "Zrnková, střední pražení, čokoláda a oříšky", new Price(10.90m, "EUR"), 140, roastedOn: Roasted(9), origin: "Kolumbie",
                imageUrl: "/images/products/kolumbie-huila.jpg"),
            Product.Create(Guid.Parse("d3a7e2c5-8b14-4f69-a0d2-6c5e1b9f4a71"), "Výběr pražírny — ovocná 250 g",
                "Zrnková, každé balení jiná káva s ovocnými tóny podle výběru pražírny", new Price(11.90m, "EUR"), 80,
                roastedOn: Roasted(5)),
            Product.Create(Guid.Parse("6e0b9f37-c2a8-4d15-b7e4-0f3a8d6c2e92"), "Výběr pražírny — čokoládová 250 g",
                "Zrnková, každé balení jiná káva s tóny čokolády a oříšků podle výběru pražírny", new Price(10.90m, "EUR"), 80,
                roastedOn: Roasted(5)),
            Product.Create(Guid.Parse("b5d84e17-2a9c-4f3b-a6e0-91c7f2d5b833"), "Brazílie Santos 1 kg",
                "Zrnková do espressa, tmavší pražení", new Price(27.90m, "EUR"), 60, roastedOn: Roasted(4), origin: "Brazílie",
                imageUrl: "/images/products/brazilie-santos.jpg"),
            Product.Create(Guid.Parse("e9a6c3d0-7b1f-4e85-9d42-3f0b8a6c1d44"), "Etiopie Yirgacheffe mletá 250 g",
                "Mletá na filtr, hrubost pro V60", new Price(12.90m, "EUR"), 45, roastedOn: Roasted(13), origin: "Etiopie",
                imageUrl: "/images/products/etiopie-mleta.jpg"),
            Product.Create(Guid.Parse("1d7f0a95-c4e2-4b68-8a3d-6e9c2f1b7a55"), "Kapsle Espresso Forte 10 ks",
                "Kompatibilní s Nespresso, intenzita 9", new Price(4.90m, "EUR"), 300, roastedOn: Roasted(21),
                imageUrl: "/images/products/kapsle-espresso.jpg"),
            Product.Create(Guid.Parse("5a3e8d26-9f1b-4c7e-b0a4-2d6f8e1c9b66"), "Kapsle Lungo Delicato 10 ks",
                "Kompatibilní s Nespresso, intenzita 5", new Price(4.90m, "EUR"), 260, roastedOn: Roasted(27),
                imageUrl: "/images/products/kapsle-lungo.jpg"),
            Product.Create(Guid.Parse("8e2b5f19-3c7a-4d0e-91b6-a4f2c8e5d077"), "Ruční mlýnek Kompakt",
                "Kuželové ocelové kameny, 24 stupňů hrubosti", new Price(89.00m, "EUR"), 18, imageUrl: "/images/products/mlynek.jpg"),
            Product.Create(Guid.Parse("6b0e3f84-2d7a-4c19-9e56-b1a8d4c7f2e0"), "Papírové filtry V60 02, 100 ks", "Jednorázové filtry",
                new Price(6.90m, "EUR"), 80, imageUrl: "/images/products/filtry-v60.jpg"),
            Product.Create(Guid.Parse("2c9d4a70-e6b3-4f81-8d5c-7b1e3a9f6088"), "Překapávač V60 02",
                "Keramický, včetně 10 papírových filtrů", new Price(24.90m, "EUR"), 40, imageUrl: "/images/products/v60.jpg"),
            Product.Create(Guid.Parse("f4b1c7e8-5a2d-4e93-b7f0-c3e9d6a2b199"), "Konvice s husím krkem 1 l", "Na sporák, matná černá",
                new Price(44.90m, "EUR"), 12, imageUrl: "/images/products/konvice.jpg"),
            Product.Create(Guid.Parse("9b6e2d53-1f8c-4a07-a5e3-8d4b0c7f2e1a"), "Sypaný čaj Darjeeling 100 g", "První sklizeň",
                new Price(8.50m, "EUR"), 70, origin: "Indie", imageUrl: "/images/products/darjeeling.jpg"),
            Product.Create(Guid.Parse("c8f3a1b4-6d9e-4c25-8e71-5b2a0f9d3c2b"), "Cupping lžíce", "Ukončená řada", new Price(9.90m, "EUR"), 3,
                isActive: false)
        );

        // Petra's id is fixed: the demo customer login in appsettings points at her.
        db.Customers.AddRange(
            Customer.Register(Guid.Parse("6f1c2a4e-8d3b-4c7a-9e21-5b0f3d8a7c14"), "Petra Nováková", new Email("petra@example.com"),
                "Kounicova 12", "Brno", "60200", DateTime.Now.AddMonths(-3), isVip: true, paymentMethodToken: "tok_visa",
                paymentCardLabel: "Visa ending 4242"),
            Customer.Register(Guid.Parse("4d8b2f61-9a3e-4c7d-b5e0-1f6a8c2d9e34"), "Jan Dvořák", new Email("jan@example.com"), "Veveří 45",
                "Brno", "60200", DateTime.Now.AddMonths(-2), paymentMethodToken: "tok_visa", paymentCardLabel: "Visa ending 4242"),
            Customer.Register(Guid.Parse("a7e3c9d1-2b5f-4e8a-9c06-3d1f7b4e8a45"), "Eva Horáková", new Email("eva@example.com"),
                "Palackého 3", "Praha", "11000", DateTime.Now.AddDays(-20), paymentMethodToken: "tok_visa",
                paymentCardLabel: "Visa ending 4242")
        );

        db.SaveChanges();

        // A few months of orders in every state.
        var petra = db.Customers.Find(Guid.Parse("6f1c2a4e-8d3b-4c7a-9e21-5b0f3d8a7c14"))!;
        var jan = db.Customers.Find(Guid.Parse("4d8b2f61-9a3e-4c7d-b5e0-1f6a8c2d9e34"))!;
        var eva = db.Customers.Find(Guid.Parse("a7e3c9d1-2b5f-4e8a-9c06-3d1f7b4e8a45"))!;
        db.Orders.AddRange(
            SeedOrder(db, petra, OrderStatus.Shipped, daysAgo: 60, ("Etiopie Yirgacheffe 250 g", 2), ("Překapávač V60 02", 1)),
            SeedOrder(db, jan, OrderStatus.Shipped, daysAgo: 41, ("Brazílie Santos 1 kg", 1)),
            SeedOrder(db, jan, OrderStatus.Cancelled, daysAgo: 30, ("Ruční mlýnek Kompakt", 1)),
            SeedOrder(db, eva, OrderStatus.Paid, daysAgo: 2, ("Kolumbie Huila 250 g", 1), ("Kapsle Lungo Delicato 10 ks", 2)),
            SeedOrder(db, petra, OrderStatus.Paid, daysAgo: 1, ("Konvice s husím krkem 1 l", 1), ("Kolumbie Huila 250 g", 2)),
            SeedOrder(db, petra, OrderStatus.New, daysAgo: 0, ("Kapsle Espresso Forte 10 ks", 3))
        );

        db.SaveChanges();
        SubscriptionSeeder.Seed(db);
    }

    private static Order SeedOrder(
        AppDbContext db, Customer customer, OrderStatus status, int daysAgo, params (string Product, int Quantity)[] lines)
    {
        var items = lines.Select(line =>
        {
            var product = db.Products.Local.Single(p => p.Name == line.Product);
            return OrderItem.Create(product.Id, product.Name, new Quantity(line.Quantity), product.Price);
        });
        var order = Order.Create(Guid.NewGuid(), customer.Id, customer.Name, customer.Email,
            customer.Street, customer.City, customer.Zip, DateTime.Now.AddDays(-daysAgo).AddHours(-3), items);

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

    // The catalogue always looks freshly roasted, whenever the demo runs.
    private static DateOnly Roasted(int daysAgo) =>
        DateOnly.FromDateTime(DateTime.Now.AddDays(-daysAgo));
}
