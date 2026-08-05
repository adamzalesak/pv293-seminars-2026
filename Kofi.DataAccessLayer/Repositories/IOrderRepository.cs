using Kofi.DataAccessLayer.Models;

namespace Kofi.DataAccessLayer.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetById(Guid id);
    Task<Order?> GetByIdWithItems(Guid id);
    IQueryable<Order> Query();
    void Add(Order order);
    void Update(Order order);
    Task<int> CountByStatus(OrderStatus status);
}
