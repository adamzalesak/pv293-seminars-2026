using System.Security.Cryptography;
using System.Text;
using Kofi.Common.Services;

namespace Kofi.Services;

// The provider signs every webhook body with the secret both sides share.
public class PaymentWebhookVerifier(IConfiguration configuration) : IPaymentWebhookVerifier
{
    public bool IsAuthentic(string body, string? signature)
    {
        var secret = configuration["PaymentGateway:WebhookSecret"];
        if (string.IsNullOrEmpty(secret) || signature is null || !signature.StartsWith("sha256="))
        {
            return false;
        }

        var expected = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(body));
        try
        {
            return CryptographicOperations.FixedTimeEquals(expected, Convert.FromHexString(signature["sha256=".Length..]));
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
