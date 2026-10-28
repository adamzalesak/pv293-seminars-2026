namespace Kofi.Ordering.API.Endpoints;

public record PaymentWebhookRequest
{
    public Guid EventId { get; init; }
    public Guid PaymentId { get; init; }
    public string Reference { get; init; } = "";
    public string Status { get; init; } = "";
    public DateTime OccurredAt { get; init; }
}
