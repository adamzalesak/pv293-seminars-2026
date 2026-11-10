namespace Kofi.Application.Features.Customers.ConfirmPaymentCardChange;

public record ConfirmPaymentCardChangeResponse
{
    public required string CardSetupStatus { get; init; }
    public required string? Label { get; init; }
}
