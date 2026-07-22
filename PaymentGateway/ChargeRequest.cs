namespace PaymentGateway;

public record ChargeRequest
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "";
    public string Reference { get; init; } = "";
    public string PaymentMethodToken { get; init; } = "";
    public string IdempotencyKey { get; init; } = "";
}
