using Kofi.DataAccessLayer.Models;

namespace Kofi.DataAccessLayer.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetById(Guid id);
    Task<Order?> GetByIdWithItems(Guid id);
    IQueryable<Order> Query();
    Task Add(Order order);
    Task Update(Order order);
    Task<int> CountByStatus(OrderStatus status);
}
