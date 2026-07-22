using Kofi.DataAccessLayer.Models;

namespace Kofi.DataAccessLayer.Data;

public static class DbSeeder
{
    // Quick and dirty seeding so the demo works. Proper migrations "later".
    public static void Seed(AppDbContext db)
    {
        db.Database.EnsureCreated();

        if (db.Products.Any())
            return;

        db.Products.AddRange(
            new Product { Id = Guid.NewGuid(), Name = "Etiopie Yirgacheffe 250 g", ImageUrl = "/images/products/etiopie-yirgacheffe.jpg", Description = "Zrnková, světlé pražení, praná metoda, tóny citrusů", Price = 12.50m, StockQuantity = 120, RoastedOn = Roasted(6), Origin = "Etiopie" },
            new Product { Id = Guid.NewGuid(), Name = "Kolumbie Huila 250 g", ImageUrl = "/images/products/kolumbie-huila.jpg", Description = "Zrnková, střední pražení, čokoláda a oříšky", Price = 10.90m, StockQuantity = 140, RoastedOn = Roasted(9), Origin = "Kolumbie" },
            new Product { Id = Guid.NewGuid(), Name = "Brazílie Santos 1 kg", ImageUrl = "/images/products/brazilie-santos.jpg", Description = "Zrnková do espressa, tmavší pražení", Price = 27.90m, StockQuantity = 60, RoastedOn = Roasted(4), Origin = "Brazílie" },
            new Product { Id = Guid.NewGuid(), Name = "Etiopie Yirgacheffe mletá 250 g", ImageUrl = "/images/products/etiopie-mleta.jpg", Description = "Mletá na filtr, hrubost pro V60", Price = 12.90m, StockQuantity = 45, RoastedOn = Roasted(13), Origin = "Etiopie" },
            new Product { Id = Guid.NewGuid(), Name = "Kapsle Espresso Forte 10 ks", ImageUrl = "/images/products/kapsle-espresso.jpg", Description = "Kompatibilní s Nespresso, intenzita 9", Price = 4.90m, StockQuantity = 300, RoastedOn = Roasted(21) },
            new Product { Id = Guid.NewGuid(), Name = "Kapsle Lungo Delicato 10 ks", ImageUrl = "/images/products/kapsle-lungo.jpg", Description = "Kompatibilní s Nespresso, intenzita 5", Price = 4.90m, StockQuantity = 260, RoastedOn = Roasted(27) },
            new Product { Id = Guid.NewGuid(), Name = "Ruční mlýnek Kompakt", ImageUrl = "/images/products/mlynek.jpg", Description = "Kuželové ocelové kameny, 24 stupňů hrubosti", Price = 89.00m, StockQuantity = 18 },
            new Product { Id = Guid.NewGuid(), Name = "Překapávač V60 02", ImageUrl = "/images/products/v60.jpg", Description = "Keramický, včetně 10 papírových filtrů", Price = 24.90m, StockQuantity = 40 },
            new Product { Id = Guid.NewGuid(), Name = "Konvice s husím krkem 1 l", ImageUrl = "/images/products/konvice.jpg", Description = "Na sporák, matná černá", Price = 44.90m, StockQuantity = 12 },
            new Product { Id = Guid.NewGuid(), Name = "Sypaný čaj Darjeeling 100 g", ImageUrl = "/images/products/darjeeling.jpg", Description = "První sklizeň", Price = 8.50m, StockQuantity = 70, Origin = "Indie" },
            new Product { Id = Guid.NewGuid(), Name = "Cupping lžíce", Description = "Ukončená řada", Price = 9.90m, StockQuantity = 3, IsActive = false }
        );

        // Petra's id is fixed: the demo customer login in appsettings points at her.
        db.Customers.AddRange(
            new Customer { Id = Guid.Parse("6f1c2a4e-8d3b-4c7a-9e21-5b0f3d8a7c14"), Name = "Petra Nováková", Email = "petra@example.com", Street = "Kounicova 12", City = "Brno", Zip = "60200", RegisteredAt = DateTime.Now.AddMonths(-3), IsVip = true },
            new Customer { Id = Guid.NewGuid(), Name = "Jan Dvořák", Email = "jan@example.com", Street = "Veveří 45", City = "Brno", Zip = "60200", RegisteredAt = DateTime.Now.AddMonths(-2) },
            new Customer { Id = Guid.NewGuid(), Name = "Eva Horáková", Email = "eva@example.com", Street = "Palackého 3", City = "Praha", Zip = "11000", RegisteredAt = DateTime.Now.AddDays(-20) }
        );

        db.SaveChanges();
    }

    // The catalogue always looks freshly roasted, whenever the demo runs.
    private static DateOnly Roasted(int daysAgo) =>
        DateOnly.FromDateTime(DateTime.Now.AddDays(-daysAgo));
}
