using Kofi.DataAccessLayer.Models;

namespace Kofi.BusinessLayer.Services;

public interface IEmailService
{
    Task SendOrderConfirmation(Order order);
    Task SendOrderCancelled(Order order);
    Task SendShippingNotification(Order order);
}
