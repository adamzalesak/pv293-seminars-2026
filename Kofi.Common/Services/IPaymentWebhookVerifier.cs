namespace Kofi.Common.Services;

// Tells a webhook the payment provider really sent from anything else that
// reaches the open endpoint.
public interface IPaymentWebhookVerifier
{
    bool IsAuthentic(string body, string? signature);
}
