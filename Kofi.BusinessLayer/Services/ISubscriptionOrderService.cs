using Kofi.Domain.Subscriptions;

namespace Kofi.BusinessLayer.Services;

public interface ISubscriptionOrderService
{
    Task Create(Guid customerId, DateTime now, PreparedDelivery delivery);
}
