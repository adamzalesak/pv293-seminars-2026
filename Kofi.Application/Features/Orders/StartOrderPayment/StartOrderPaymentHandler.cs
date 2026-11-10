using Kofi.Application.Abstractions;
using Kofi.Application.Repositories;
using Kofi.Application.Services;
using Kofi.Domain;
using Kofi.Domain.Orders;

namespace Kofi.Application.Features.Orders.StartOrderPayment;

// The customer pays on the provider's page. We only learn the result when we
// ask the provider ourselves, in ConfirmOrderPayment.
public class StartOrderPaymentHandler(
    IOrderRepository orders,
    IPricingService pricing,
    IPaymentGatewayClient payments,
    ICurrentUser user) : ICommandHandler<StartOrderPaymentCommand, StartOrderPaymentResponse>
{
    public async Task<StartOrderPaymentResponse> Handle(StartOrderPaymentCommand command, CancellationToken ct)
    {
        var order = await orders.GetById(command.Id);

        // Only the customer who placed the order pays for it.
        if (order == null || order.CustomerId != user.CustomerId)
        {
            throw new NotFoundException("Order not found");
        }

        if (order.Status != OrderStatus.New)
        {
            throw new DomainRuleException("Only new orders can be paid");
        }

        // While the last attempt is still open, the customer gets the same
        // payment page back. A new attempt only after a decline.
        if (order.PaymentId != null)
        {
            var previous = await payments.GetPayment(order.PaymentId.Value);
            if (previous.Status == PaymentStatus.Created)
            {
                return new StartOrderPaymentResponse { PaymentId = order.PaymentId.Value, RedirectUrl = previous.RedirectUrl };
            }

            if (previous.Status == PaymentStatus.Captured)
            {
                // Paid, but the customer never came back from the payment
                // page: send them back to it now, and it records the payment.
                return new StartOrderPaymentResponse
                {
                    PaymentId = order.PaymentId.Value,
                    RedirectUrl = $"{command.ReturnUrl}?paymentId={order.PaymentId}",
                };
            }
        }

        var attempt = order.PaymentId == null ? order.Id.ToString() : $"{order.Id}/{order.PaymentId}";

        var itemsTotal = pricing.CalculateItemsTotal(order.Items);
        var total = Math.Round(itemsTotal + pricing.CalculateShipping(itemsTotal), 2);
        var payment = await payments.CreatePayment(total, "EUR", order.Id.ToString(), command.ReturnUrl, attempt);

        order.StartPayment(payment.PaymentId);
        return new StartOrderPaymentResponse { PaymentId = payment.PaymentId, RedirectUrl = payment.RedirectUrl };
    }
}
