using Kofi.Warehouse.Domain.Products;

namespace Kofi.Warehouse.Infrastructure.Persistence;

internal static class DemoData
{
    public static void Seed(WarehouseDbContext db)
    {
        if (db.Products.Any())
        {
            return;
        }

        db.Products.AddRange(
            Product.Create(Guid.Parse("3f2a8c61-5d4e-4b7a-9e1c-0a6b2d8f4e11"), 120, "A-01"),
            Product.Create(Guid.Parse("7c1e9b42-0f3d-4a6e-8b25-d4f1a9c3e522"), 140, "A-01"),
            Product.Create(Guid.Parse("d3a7e2c5-8b14-4f69-a0d2-6c5e1b9f4a71"), 80, "A-01"),
            Product.Create(Guid.Parse("6e0b9f37-c2a8-4d15-b7e4-0f3a8d6c2e92"), 80, "A-01"),
            Product.Create(Guid.Parse("b5d84e17-2a9c-4f3b-a6e0-91c7f2d5b833"), 60, "A-01"),
            Product.Create(Guid.Parse("e9a6c3d0-7b1f-4e85-9d42-3f0b8a6c1d44"), 45, "A-01"),
            Product.Create(Guid.Parse("1d7f0a95-c4e2-4b68-8a3d-6e9c2f1b7a55"), 300, "A-01"),
            Product.Create(Guid.Parse("5a3e8d26-9f1b-4c7e-b0a4-2d6f8e1c9b66"), 260, "A-01"),
            Product.Create(Guid.Parse("8e2b5f19-3c7a-4d0e-91b6-a4f2c8e5d077"), 18, "A-01"),
            Product.Create(Guid.Parse("6b0e3f84-2d7a-4c19-9e56-b1a8d4c7f2e0"), 80, "A-01"),
            Product.Create(Guid.Parse("2c9d4a70-e6b3-4f81-8d5c-7b1e3a9f6088"), 40, "A-01"),
            Product.Create(Guid.Parse("f4b1c7e8-5a2d-4e93-b7f0-c3e9d6a2b199"), 12, "A-01"),
            Product.Create(Guid.Parse("9b6e2d53-1f8c-4a07-a5e3-8d4b0c7f2e1a"), 70, "A-01"),
            Product.Create(Guid.Parse("c8f3a1b4-6d9e-4c25-8e71-5b2a0f9d3c2b"), 3, "A-01")
        );
        db.SaveChanges();
    }
}
