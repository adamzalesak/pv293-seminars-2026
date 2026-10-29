using Kofi.Common.Services;
using Wolverine.Attributes;

namespace Kofi.Ordering.Application.Payments;

// The call to the provider, outside any database transaction: nothing waits
// for the bank while holding locks. If it fails, Wolverine tries again with
// the same idempotency key. The result comes later, by webhook.
[NonTransactional]
public class ChargeCardHandler(IPaymentGatewayClient payments)
{
    public async Task Handle(ChargeCard charge, CancellationToken ct) =>
        await payments.Charge(charge.Amount, charge.Currency, charge.OrderId.ToString(), charge.PaymentMethodToken,
            charge.IdempotencyKey, confirmByWebhook: true);
}
