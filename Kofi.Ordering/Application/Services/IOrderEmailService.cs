using Kofi.Ordering.Domain.Orders;

namespace Kofi.Ordering.Application.Services;

public interface IOrderEmailService
{
    Task SendOrderConfirmation(Order order);
    Task SendOrderCancelled(Order order);
    Task SendShippingNotification(Order order);
}
