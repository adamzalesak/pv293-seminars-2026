using System.Net.Mail;
using Kofi.Common.Services;

namespace Kofi.Services;

public class EmailTransport(ILogger<EmailTransport> logger, IConfiguration config) : IEmailTransport
{
    public async Task Send(string to, string subject, string body)
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
