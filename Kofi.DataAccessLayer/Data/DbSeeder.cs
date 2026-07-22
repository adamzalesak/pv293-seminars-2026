using Kofi.DataAccessLayer.Models;

namespace Kofi.DataAccessLayer.Data;

public static class DbSeeder
{
    // Creates the schema and seeds the demo data on startup.
    public static void Seed(AppDbContext db)
    {
        db.Database.EnsureCreated();

        if (db.Products.Any())
            return;

        // Fixed ids, so the requests in kofi.http keep working after a restart.
        db.Products.AddRange(
            new Product { Id = Guid.Parse("3f2a8c61-5d4e-4b7a-9e1c-0a6b2d8f4e11"), Name = "Etiopie Yirgacheffe 250 g", ImageUrl = "/images/products/etiopie-yirgacheffe.jpg", Description = "Zrnková, světlé pražení, praná metoda, tóny citrusů", Price = 12.50m, StockQuantity = 120, RoastedOn = Roasted(6), Origin = "Etiopie" },
            new Product { Id = Guid.Parse("7c1e9b42-0f3d-4a6e-8b25-d4f1a9c3e522"), Name = "Kolumbie Huila 250 g", ImageUrl = "/images/products/kolumbie-huila.jpg", Description = "Zrnková, střední pražení, čokoláda a oříšky", Price = 10.90m, StockQuantity = 140, RoastedOn = Roasted(9), Origin = "Kolumbie" },
            new Product { Id = Guid.Parse("b5d84e17-2a9c-4f3b-a6e0-91c7f2d5b833"), Name = "Brazílie Santos 1 kg", ImageUrl = "/images/products/brazilie-santos.jpg", Description = "Zrnková do espressa, tmavší pražení", Price = 27.90m, StockQuantity = 60, RoastedOn = Roasted(4), Origin = "Brazílie" },
            new Product { Id = Guid.Parse("e9a6c3d0-7b1f-4e85-9d42-3f0b8a6c1d44"), Name = "Etiopie Yirgacheffe mletá 250 g", ImageUrl = "/images/products/etiopie-mleta.jpg", Description = "Mletá na filtr, hrubost pro V60", Price = 12.90m, StockQuantity = 45, RoastedOn = Roasted(13), Origin = "Etiopie" },
            new Product { Id = Guid.Parse("1d7f0a95-c4e2-4b68-8a3d-6e9c2f1b7a55"), Name = "Kapsle Espresso Forte 10 ks", ImageUrl = "/images/products/kapsle-espresso.jpg", Description = "Kompatibilní s Nespresso, intenzita 9", Price = 4.90m, StockQuantity = 300, RoastedOn = Roasted(21) },
            new Product { Id = Guid.Parse("5a3e8d26-9f1b-4c7e-b0a4-2d6f8e1c9b66"), Name = "Kapsle Lungo Delicato 10 ks", ImageUrl = "/images/products/kapsle-lungo.jpg", Description = "Kompatibilní s Nespresso, intenzita 5", Price = 4.90m, StockQuantity = 260, RoastedOn = Roasted(27) },
            new Product { Id = Guid.Parse("8e2b5f19-3c7a-4d0e-91b6-a4f2c8e5d077"), Name = "Ruční mlýnek Kompakt", ImageUrl = "/images/products/mlynek.jpg", Description = "Kuželové ocelové kameny, 24 stupňů hrubosti", Price = 89.00m, StockQuantity = 18 },
            new Product { Id = Guid.Parse("2c9d4a70-e6b3-4f81-8d5c-7b1e3a9f6088"), Name = "Překapávač V60 02", ImageUrl = "/images/products/v60.jpg", Description = "Keramický, včetně 10 papírových filtrů", Price = 24.90m, StockQuantity = 40 },
            new Product { Id = Guid.Parse("f4b1c7e8-5a2d-4e93-b7f0-c3e9d6a2b199"), Name = "Konvice s husím krkem 1 l", ImageUrl = "/images/products/konvice.jpg", Description = "Na sporák, matná černá", Price = 44.90m, StockQuantity = 12 },
            new Product { Id = Guid.Parse("9b6e2d53-1f8c-4a07-a5e3-8d4b0c7f2e1a"), Name = "Sypaný čaj Darjeeling 100 g", ImageUrl = "/images/products/darjeeling.jpg", Description = "První sklizeň", Price = 8.50m, StockQuantity = 70, Origin = "Indie" },
            new Product { Id = Guid.Parse("c8f3a1b4-6d9e-4c25-8e71-5b2a0f9d3c2b"), Name = "Cupping lžíce", Description = "Ukončená řada", Price = 9.90m, StockQuantity = 3, IsActive = false }
        );

        // Petra's id is fixed: the demo customer login in appsettings points at her.
        db.Customers.AddRange(
            new Customer { Id = Guid.Parse("6f1c2a4e-8d3b-4c7a-9e21-5b0f3d8a7c14"), Name = "Petra Nováková", Email = "petra@example.com", Street = "Kounicova 12", City = "Brno", Zip = "60200", RegisteredAt = DateTime.Now.AddMonths(-3), IsVip = true },
            new Customer { Id = Guid.Parse("4d8b2f61-9a3e-4c7d-b5e0-1f6a8c2d9e34"), Name = "Jan Dvořák", Email = "jan@example.com", Street = "Veveří 45", City = "Brno", Zip = "60200", RegisteredAt = DateTime.Now.AddMonths(-2) },
            new Customer { Id = Guid.Parse("a7e3c9d1-2b5f-4e8a-9c06-3d1f7b4e8a45"), Name = "Eva Horáková", Email = "eva@example.com", Street = "Palackého 3", City = "Praha", Zip = "11000", RegisteredAt = DateTime.Now.AddDays(-20) }
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
    }

    private static Order SeedOrder(
        AppDbContext db, Customer customer, OrderStatus status, int daysAgo, params (string Product, int Quantity)[] lines)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            CustomerName = customer.Name,
            CustomerEmail = customer.Email,
            Street = customer.Street,
            City = customer.City,
            Zip = customer.Zip,
            Status = status,
            CreatedAt = DateTime.Now.AddDays(-daysAgo).AddHours(-3),
        };
        foreach (var (name, quantity) in lines)
        {
            var product = db.Products.Local.Single(p => p.Name == name);
            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = quantity,
            });
        }
        return order;
    }

    // The catalogue always looks freshly roasted, whenever the demo runs.
    private static DateOnly Roasted(int daysAgo) =>
        DateOnly.FromDateTime(DateTime.Now.AddDays(-daysAgo));
}
