using Kofi.BusinessLayer.Abstractions;

namespace Kofi.BusinessLayer.Features.Orders.ConfirmOrderPayment;

public record ConfirmOrderPaymentCommand : ICommand<ConfirmOrderPaymentResponse>
{
    public Guid Id { get; init; }
    public Guid PaymentId { get; init; }
}
