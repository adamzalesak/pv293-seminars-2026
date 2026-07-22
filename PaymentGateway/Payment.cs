namespace PaymentGateway;

public class Payment
{
    public required Guid Id { get; init; }
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
    public required string Reference { get; init; }
    public string? ReturnUrl { get; init; }
    public string? RedirectUrl { get; set; }
    public required DateTime CreatedAt { get; init; }
    public PaymentStatus Status { get; set; }
}
