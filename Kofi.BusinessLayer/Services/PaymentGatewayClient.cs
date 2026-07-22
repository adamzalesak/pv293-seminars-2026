using System.Net.Http.Json;
using System.Text.Json;

namespace Kofi.BusinessLayer.Services;

// Talks to the card payment provider. Aspire tells HttpClient where it runs.
public class PaymentGatewayClient(HttpClient http)
{
    // The key names one payment attempt: a retried request with the same key
    // gets the same payment back, a new attempt needs a new key.
    public async Task<(Guid PaymentId, string RedirectUrl)> CreatePayment(
        decimal amount, string currency, string reference, string returnUrl, string idempotencyKey)
    {
        var response = await http.PostAsJsonAsync("/payments", new
        {
            amount,
            currency,
            reference,
            returnUrl,
            idempotencyKey,
        });
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return (json.GetProperty("id").GetGuid(), json.GetProperty("redirectUrl").GetString()!);
    }

    // Stops a payment the customer has not made yet. One the customer made
    // in the meantime cannot be stopped any more, so its money goes back.
    // Returns the payment's final status: Cancelled, Refunded, or Declined
    // for an attempt that never took money.
    public async Task<string> CancelOrRefund(Guid paymentId)
    {
        var status = await Post($"/payments/{paymentId}/cancel");
        return status == "Captured" ? await Post($"/payments/{paymentId}/refund") : status;
    }

    private async Task<string> Post(string path)
    {
        var response = await http.PostAsync(path, null);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("status").GetString()!;
    }

    public async Task<string> GetStatus(Guid paymentId) => (await GetPayment(paymentId)).Status;

    public async Task<(string Status, string RedirectUrl)> GetPayment(Guid paymentId)
    {
        var json = await http.GetFromJsonAsync<JsonElement>($"/payments/{paymentId}");
        return (json.GetProperty("status").GetString()!, json.GetProperty("redirectUrl").GetString() ?? "");
    }
}
