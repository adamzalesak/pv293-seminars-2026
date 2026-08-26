namespace PaymentGateway;

public record CreateSetupRequest
{
    public string Reference { get; init; } = "";
    public string ReturnUrl { get; init; } = "";
    public string IdempotencyKey { get; init; } = "";
}
