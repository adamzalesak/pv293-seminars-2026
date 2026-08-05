namespace Kofi.BusinessLayer.Features.Orders.ConfirmOrderPayment;

public record ConfirmOrderPaymentResponse
{
    public required Guid OrderId { get; init; }
    public required string Status { get; init; }
}
