using Kofi.Domain.Subscriptions;

namespace Kofi.Application.Services;

public interface ISubscriptionOrderService
{
    Task Create(Guid customerId, DateTime now, PreparedDelivery delivery);
}
