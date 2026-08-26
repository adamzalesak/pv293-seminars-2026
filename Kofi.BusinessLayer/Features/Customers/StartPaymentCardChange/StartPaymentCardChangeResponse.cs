namespace Kofi.BusinessLayer.Features.Customers.StartPaymentCardChange;

public record StartPaymentCardChangeResponse
{
    public required Guid SetupId { get; init; }
    public required string RedirectUrl { get; init; }
}
