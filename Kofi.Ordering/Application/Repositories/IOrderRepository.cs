using Kofi.Ordering.Domain.Orders;

namespace Kofi.Ordering.Application.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetById(Guid id);
    void Add(Order order);
}
