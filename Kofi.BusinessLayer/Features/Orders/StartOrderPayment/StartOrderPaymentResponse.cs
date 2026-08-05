namespace Kofi.BusinessLayer.Features.Orders.StartOrderPayment;

public record StartOrderPaymentResponse
{
    public required Guid PaymentId { get; init; }
    public required string RedirectUrl { get; init; }
}
