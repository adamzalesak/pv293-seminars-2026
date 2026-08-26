namespace PaymentGateway;

// The customer's permission to charge a card later, with nobody present.
// Only a succeeded setup carries the token the shop may charge.
public class CardSetup
{
    public required Guid Id { get; init; }
    public required string Reference { get; init; }
    public required string ReturnUrl { get; init; }
    public string? RedirectUrl { get; set; }
    public required DateTime CreatedAt { get; init; }
    public SetupStatus Status { get; set; }
    public string? PaymentMethodToken { get; set; }
    public string? CardLabel { get; set; }
}
