using Kofi.Common.Services;
using Kofi.Ordering.Application.Repositories;
using Kofi.Ordering.Application.Sagas;
using Wolverine.Attributes;

namespace Kofi.Ordering.Application.Payments;

// The same webhook delivered twice runs once ([Deduplicated] on EventId); the
// same result reaching us by webhook and by redirect is caught by the order's
// own state.
[Deduplicated]
public class PaymentResultReceivedHandler(IOrderRepository orders, IPaymentGatewayClient payments)
{
    public async Task<DeliveryPaymentDecided?> Handle(PaymentResultReceived result, CancellationToken ct)
    {
        var order = await orders.GetById(result.OrderId);

        // A payment for an order the shop never saved: nothing to confirm.
        if (order == null)
        {
            return null;
        }

        // An order for a subscription delivery belongs to its saga, which
        // decides what a result means.
        if (order.DeliveryId is { } deliveryId)
        {
            return new DeliveryPaymentDecided(deliveryId, result.PaymentId, result.Status == nameof(PaymentStatus.Captured));
        }

        if (order.PaymentId != result.PaymentId)
        {
            return null;
        }

        if (order.AcceptPaymentResult(result.PaymentId, result.Status == nameof(PaymentStatus.Captured)))
        {
            await payments.CancelOrRefund(result.PaymentId);
        }

        return null;
    }
}
