namespace Kofi.Application.Features.Customers.GetPaymentCard;

public record PaymentCard
{
    // Null until the customer authorises a card.
    public required string? Label { get; init; }
}
