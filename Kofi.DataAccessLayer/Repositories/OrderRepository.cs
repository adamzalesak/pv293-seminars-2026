using Microsoft.EntityFrameworkCore;
using Kofi.DataAccessLayer.Data;
using Kofi.DataAccessLayer.Models;

namespace Kofi.DataAccessLayer.Repositories;

public class OrderRepository(AppDbContext db)
{
    public async Task<Order?> GetById(Guid id)
    {
        return await db.Orders.FirstOrDefaultAsync(o => o.Id == id);
    }

    // Had to add Product after the detail started reading prices off it, and
    // then Customer for the admin view.
    public async Task<Order?> GetByIdWithItems(Guid id)
    {
        return await db.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<List<Order>> GetAll()
    {
        return await db.Orders.ToListAsync();
    }

    // The service needs to filter and sort in five different ways, so it gets
    // the queryable and does it there.
    public IQueryable<Order> Query()
    {
        return db.Orders;
    }

    public async Task Add(Order order)
    {
        db.Orders.Add(order);
        await db.SaveChangesAsync();
    }

    public async Task Update(Order order)
    {
        db.Orders.Update(order);
        await db.SaveChangesAsync();
    }

    public async Task<int> CountByStatus(OrderStatus status)
    {
        return await db.Orders.CountAsync(o => o.Status == status);
    }
}
