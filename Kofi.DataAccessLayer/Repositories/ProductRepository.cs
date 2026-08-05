using Microsoft.EntityFrameworkCore;
using Kofi.DataAccessLayer.Data;
using Kofi.DataAccessLayer.Models;

namespace Kofi.DataAccessLayer.Repositories;

public class ProductRepository(AppDbContext db) : IProductRepository
{
    public async Task<Product?> GetById(Guid id)
    {
        return await db.Products.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<(List<Product> Items, int TotalCount)> Search(
        string? term, string? sort, int page, int pageSize, bool includeInactive)
    {
        var query = includeInactive ? db.Products : db.Products.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(term))
        {
            query = query.Where(p =>
                EF.Functions.ILike(p.Name, $"%{term}%")
                || EF.Functions.ILike(p.Description, $"%{term}%"));
        }

        query = sort switch
        {
            "price" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            _ => query.OrderBy(p => p.Name),
        };

        var totalCount = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, totalCount);
    }

    public void Add(Product product)
    {
        db.Products.Add(product);
    }

    public void Update(Product product)
    {
        db.Products.Update(product);
    }
}
