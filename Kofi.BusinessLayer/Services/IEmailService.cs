using Kofi.DataAccessLayer.Models;

namespace Kofi.BusinessLayer.Services;

public interface IEmailService
{
    Task SendOrderConfirmation(Order order);
    Task SendSubscriptionActivated(Subscription subscription);
    Task SendOrderCancelled(Order order);
    Task SendShippingNotification(Order order);
}
