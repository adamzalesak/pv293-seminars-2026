using Kofi.Application.Repositories;
using Kofi.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Infrastructure.Persistence.Repositories;

public class OrderRepository(AppDbContext db) : IOrderRepository
{
    public Task<Order?> GetById(Guid id) => db.Orders.FirstOrDefaultAsync(o => o.Id == id);
    public void Add(Order order) => db.Orders.Add(order);
}
