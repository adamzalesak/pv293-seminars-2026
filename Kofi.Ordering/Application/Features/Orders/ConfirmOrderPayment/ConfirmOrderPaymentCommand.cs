using Kofi.Common.Abstractions;

namespace Kofi.Ordering.Application.Features.Orders.ConfirmOrderPayment;

public record ConfirmOrderPaymentCommand : ICommand<ConfirmOrderPaymentResponse>
{
    public Guid Id { get; init; }
    public Guid PaymentId { get; init; }
}
