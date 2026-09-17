namespace Kofi.Common.Services;

public interface IEmailTransport
{
    Task Send(string to, string subject, string body);
}
