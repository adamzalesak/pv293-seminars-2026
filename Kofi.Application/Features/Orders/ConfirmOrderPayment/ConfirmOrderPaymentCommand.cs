using Kofi.Application.Abstractions;

namespace Kofi.Application.Features.Orders.ConfirmOrderPayment;

public record ConfirmOrderPaymentCommand : ICommand<ConfirmOrderPaymentResponse>
{
    public Guid Id { get; init; }
    public Guid PaymentId { get; init; }
}
