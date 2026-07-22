namespace PaymentGateway;

public record CreatePaymentRequest
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "";
    public string Reference { get; init; } = "";
    public string ReturnUrl { get; init; } = "";
    public string IdempotencyKey { get; init; } = "";
}
