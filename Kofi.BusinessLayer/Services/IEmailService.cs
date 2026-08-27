using Kofi.DataAccessLayer.Models;

namespace Kofi.BusinessLayer.Services;

public interface IEmailService
{
    Task SendOrderConfirmation(Order order);
    Task SendSubscriptionActivated(WelcomeEmail welcome);
    Task SendOrderCancelled(Order order);
    Task SendShippingNotification(Order order);
}
