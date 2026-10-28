namespace PaymentGateway;

public record ChargeRequest
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "";
    public string Reference { get; init; } = "";
    public string PaymentMethodToken { get; init; } = "";
    public string IdempotencyKey { get; init; } = "";

    // "webhook": answer at once with a pending payment and send the result
    // later by webhook. Anything else: the result is in the response.
    public string? ConfirmBy { get; init; }
}
