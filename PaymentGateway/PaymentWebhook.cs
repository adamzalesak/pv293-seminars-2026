namespace PaymentGateway;

// What the provider tells the shop once it has decided about a payment.
// EventId names this one notification: a retry of it carries the same id.
public record PaymentWebhook
{
    public required Guid EventId { get; init; }
    public required Guid PaymentId { get; init; }
    public required string Reference { get; init; }
    public required PaymentStatus Status { get; init; }
    public required DateTime OccurredAt { get; init; }
}
