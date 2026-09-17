using Kofi.Domain.Products;
using Kofi.Domain.Shared;

namespace Kofi.Infrastructure.Persistence;

internal static class DemoData
{
    public static void Seed(AppDbContext legacy)
    {
        if (legacy.Products.Any())
        {
            return;
        }

        legacy.Products.AddRange(
            Product.Create(Guid.Parse("3f2a8c61-5d4e-4b7a-9e1c-0a6b2d8f4e11"), "Etiopie Yirgacheffe 250 g",
                "Zrnková, světlé pražení, praná metoda, tóny citrusů", new Price(12.50m, "EUR"), 120, status: ProductStatus.Active,
                roastedOn: Roasted(6), origin: "Etiopie", imageUrl: "/images/products/etiopie-yirgacheffe.jpg"),
            Product.Create(Guid.Parse("7c1e9b42-0f3d-4a6e-8b25-d4f1a9c3e522"), "Kolumbie Huila 250 g",
                "Zrnková, střední pražení, čokoláda a oříšky", new Price(10.90m, "EUR"), 140, status: ProductStatus.Active,
                roastedOn: Roasted(9), origin: "Kolumbie", imageUrl: "/images/products/kolumbie-huila.jpg"),
            Product.Create(Guid.Parse("d3a7e2c5-8b14-4f69-a0d2-6c5e1b9f4a71"), "Výběr pražírny — ovocná 250 g",
                "Zrnková, každé balení jiná káva s ovocnými tóny podle výběru pražírny", new Price(11.90m, "EUR"), 80,
                status: ProductStatus.Active, roastedOn: Roasted(5)),
            Product.Create(Guid.Parse("6e0b9f37-c2a8-4d15-b7e4-0f3a8d6c2e92"), "Výběr pražírny — čokoládová 250 g",
                "Zrnková, každé balení jiná káva s tóny čokolády a oříšků podle výběru pražírny", new Price(10.90m, "EUR"), 80,
                status: ProductStatus.Active, roastedOn: Roasted(5)),
            Product.Create(Guid.Parse("b5d84e17-2a9c-4f3b-a6e0-91c7f2d5b833"), "Brazílie Santos 1 kg",
                "Zrnková do espressa, tmavší pražení", new Price(27.90m, "EUR"), 60, status: ProductStatus.Active, roastedOn: Roasted(4),
                origin: "Brazílie", imageUrl: "/images/products/brazilie-santos.jpg"),
            Product.Create(Guid.Parse("e9a6c3d0-7b1f-4e85-9d42-3f0b8a6c1d44"), "Etiopie Yirgacheffe mletá 250 g",
                "Mletá na filtr, hrubost pro V60", new Price(12.90m, "EUR"), 45, status: ProductStatus.Active, roastedOn: Roasted(13),
                origin: "Etiopie", imageUrl: "/images/products/etiopie-mleta.jpg"),
            Product.Create(Guid.Parse("1d7f0a95-c4e2-4b68-8a3d-6e9c2f1b7a55"), "Kapsle Espresso Forte 10 ks",
                "Kompatibilní s Nespresso, intenzita 9", new Price(4.90m, "EUR"), 300, status: ProductStatus.Active, roastedOn: Roasted(21),
                imageUrl: "/images/products/kapsle-espresso.jpg"),
            Product.Create(Guid.Parse("5a3e8d26-9f1b-4c7e-b0a4-2d6f8e1c9b66"), "Kapsle Lungo Delicato 10 ks",
                "Kompatibilní s Nespresso, intenzita 5", new Price(4.90m, "EUR"), 260, status: ProductStatus.Active, roastedOn: Roasted(27),
                imageUrl: "/images/products/kapsle-lungo.jpg"),
            Product.Create(Guid.Parse("8e2b5f19-3c7a-4d0e-91b6-a4f2c8e5d077"), "Ruční mlýnek Kompakt",
                "Kuželové ocelové kameny, 24 stupňů hrubosti", new Price(89.00m, "EUR"), 18, status: ProductStatus.Active,
                imageUrl: "/images/products/mlynek.jpg"),
            Product.Create(Guid.Parse("6b0e3f84-2d7a-4c19-9e56-b1a8d4c7f2e0"), "Papírové filtry V60 02, 100 ks", "Jednorázové filtry",
                new Price(6.90m, "EUR"), 80, status: ProductStatus.Active, imageUrl: "/images/products/filtry-v60.jpg"),
            Product.Create(Guid.Parse("2c9d4a70-e6b3-4f81-8d5c-7b1e3a9f6088"), "Překapávač V60 02",
                "Keramický, včetně 10 papírových filtrů", new Price(24.90m, "EUR"), 40, status: ProductStatus.Active,
                imageUrl: "/images/products/v60.jpg"),
            Product.Create(Guid.Parse("f4b1c7e8-5a2d-4e93-b7f0-c3e9d6a2b199"), "Konvice s husím krkem 1 l", "Na sporák, matná černá",
                new Price(44.90m, "EUR"), 12, status: ProductStatus.Active, imageUrl: "/images/products/konvice.jpg"),
            Product.Create(Guid.Parse("9b6e2d53-1f8c-4a07-a5e3-8d4b0c7f2e1a"), "Sypaný čaj Darjeeling 100 g", "První sklizeň",
                new Price(8.50m, "EUR"), 70, status: ProductStatus.Active, origin: "Indie", imageUrl: "/images/products/darjeeling.jpg"),
            Product.Create(Guid.Parse("c8f3a1b4-6d9e-4c25-8e71-5b2a0f9d3c2b"), "Cupping lžíce", "Ukončená řada", new Price(9.90m, "EUR"), 3,
                status: ProductStatus.Inactive)
        );

        legacy.SaveChanges();
    }

    private static DateOnly Roasted(int daysAgo) => DateOnly.FromDateTime(DateTime.Now.AddDays(-daysAgo));
}
