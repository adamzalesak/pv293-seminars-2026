using Kofi.Domain.Orders;

namespace Kofi.Application.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetById(Guid id);
    void Add(Order order);
}
