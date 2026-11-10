using Kofi.Ordering.Application.Repositories;
using Kofi.Ordering.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Ordering.Infrastructure.Persistence.Repositories;

public class OrderRepository(OrderingDbContext db) : IOrderRepository
{
    public Task<Order?> GetById(Guid id) => db.Orders.FirstOrDefaultAsync(o => o.Id == id);
    public void Add(Order order) => db.Orders.Add(order);
}
