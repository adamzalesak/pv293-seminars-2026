using Kofi.DataAccessLayer.Models;

namespace Kofi.BusinessLayer.Services;

public interface ISubscriptionOrderService
{
    Task Create(Guid customerId, Guid orderId, DateTime now, IReadOnlyList<SubscriptionOrderLine> lines);
}
