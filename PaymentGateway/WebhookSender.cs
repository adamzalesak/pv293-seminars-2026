using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PaymentGateway;

// Tells the shop about every decision, signed with the secret both sides
// share. A failed delivery is tried again a few times, like real providers
// do; the shop must therefore expect the same EventId more than once.
public class WebhookSender(IHttpClientFactory clients, IConfiguration configuration, ILogger<WebhookSender> logger)
{
    private static readonly TimeSpan[] Retries = [TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(25)];
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
    };

    public void Notify(Payment payment)
    {
        var url = configuration["Webhook:Url"];
        if (string.IsNullOrEmpty(url))
        {
            return;
        }

        var webhook = new PaymentWebhook
        {
            EventId = Guid.NewGuid(),
            PaymentId = payment.Id,
            Reference = payment.Reference,
            Status = payment.Status,
            OccurredAt = DateTime.UtcNow,
        };
        _ = Task.Run(() => Deliver(url, webhook));
    }

    public static string Sign(string body, string secret) =>
        "sha256=" + Convert.ToHexStringLower(HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(body)));

    private async Task Deliver(string url, PaymentWebhook webhook)
    {
        // Sent a moment after the decision, as providers do: the customer's
        // browser usually reaches the shop first.
        await Task.Delay(TimeSpan.FromSeconds(1));
        var body = JsonSerializer.Serialize(webhook, Json);
        var signature = Sign(body, configuration["Webhook:Secret"] ?? "");

        for (var attempt = 0; ; attempt++)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json"),
                };
                request.Headers.Add("X-Signature", signature);
                using var response = await clients.CreateClient().SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return;
                }

                logger.LogWarning("Webhook {EventId} answered {Status}", webhook.EventId, (int)response.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                logger.LogWarning("Webhook {EventId} failed: {Message}", webhook.EventId, ex.Message);
            }

            if (attempt == Retries.Length)
            {
                logger.LogError("Webhook {EventId} given up", webhook.EventId);
                return;
            }

            await Task.Delay(Retries[attempt]);
        }
    }
}
