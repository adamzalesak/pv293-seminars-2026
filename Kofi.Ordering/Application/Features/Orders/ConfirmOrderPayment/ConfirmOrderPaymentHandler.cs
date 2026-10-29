using Kofi.Common.Abstractions;
using Kofi.Common.Services;
using Kofi.Ordering.Application.Repositories;
using Kofi.Ordering.Domain.Orders;

namespace Kofi.Ordering.Application.Features.Orders.ConfirmOrderPayment;

public class ConfirmOrderPaymentHandler(
    IOrderRepository orders,
    IPaymentGatewayClient payments) : ICommandHandler<ConfirmOrderPaymentCommand, ConfirmOrderPaymentResponse>
{
    public async Task<ConfirmOrderPaymentResponse> Handle(ConfirmOrderPaymentCommand command, CancellationToken ct)
    {
        var order = await orders.GetById(command.Id);
        if (order == null || order.PaymentId != command.PaymentId)
        {
            throw new NotFoundException("Unknown payment");
        }

        // Never trust the redirect: ask the provider what really happened.
        var status = await payments.GetStatus(command.PaymentId);

        if (order.AcceptPaymentResult(command.PaymentId, status == PaymentStatus.Captured))
        {
            // The order was cancelled meanwhile. Money that was taken goes back.
            await payments.CancelOrRefund(command.PaymentId);
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            return new ConfirmOrderPaymentResponse { OrderId = order.Id, Status = PaymentOutcome.Cancelled.ToString() };
        }

        // The customer is told the shop's outcome, not the provider's own words.
        var outcome = status switch
        {
            PaymentStatus.Captured => PaymentOutcome.Paid,
            PaymentStatus.Created => PaymentOutcome.Pending,
            PaymentStatus.Declined => PaymentOutcome.Declined,
            _ => PaymentOutcome.Cancelled,
        };

        return new ConfirmOrderPaymentResponse { OrderId = order.Id, Status = outcome.ToString() };
    }
}
