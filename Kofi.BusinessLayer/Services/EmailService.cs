using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Kofi.DataAccessLayer.Models;

namespace Kofi.BusinessLayer.Services;

public class EmailService(ILogger<EmailService> logger, IConfiguration config, IPricingService pricing) : IEmailService
{
    public async Task SendOrderConfirmation(Order order)
    {
        var itemsTotal = pricing.CalculateItemsTotal(order.Items);
        var shipping = pricing.CalculateShipping(itemsTotal);

        var total = itemsTotal + shipping;

        var body = $"""
            Hello {order.CustomerName},

            thank you for your order {order.Id}.

            Items:     {itemsTotal:N2} EUR
            Shipping:  {shipping:N2} EUR
            Total:     {total:N2} EUR

            Kofi
            """;

        await Send(order.CustomerEmail, $"Your KoFI order {order.Id}", body);
    }

    public async Task SendOrderCancelled(Order order)
    {
        await Send(order.CustomerEmail, $"Order {order.Id} cancelled",
            $"Your order {order.Id} was cancelled.");
    }

    public async Task SendShippingNotification(Order order)
    {
        await Send(order.CustomerEmail, $"Order {order.Id} shipped",
            $"Your order {order.Id} has been shipped.");
    }

    // Aspire runs a MailPit test inbox and hands us its address. Without it
    // (a bare `dotnet run`), the e-mail only goes to the log.
    private async Task Send(string to, string subject, string body)
    {
        var connection = config.GetConnectionString("mail");
        if (connection == null)
        {
            logger.LogInformation("E-mail to {Email}: {Subject}\n{Body}", to, subject, body);
            return;
        }

        var endpoint = new Uri(connection.Replace("Endpoint=", ""));
        using var client = new SmtpClient(endpoint.Host, endpoint.Port);
        await client.SendMailAsync(new MailMessage("orders@kofi.test", to, subject, body));
    }
}
