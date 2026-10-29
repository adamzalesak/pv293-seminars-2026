using Kofi.Common.Abstractions;
using Kofi.Marketing.Application.Persistence;
using Kofi.Marketing.Domain.Coupons;
using Kofi.Ordering.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Marketing.Application.EventHandlers;

public class RecordCouponUseOnOrderPaid(IMarketingDbContext db) : IIntegrationEventHandler<OrderPaidIntegrationEvent>
{
    public async Task Handle(OrderPaidIntegrationEvent paid, CancellationToken ct)
    {
        if (paid.CouponCode is null)
        {
            return;
        }

        var code = new CouponCode(paid.CouponCode);
        // The order was paid with this code: record it, never refuse the fact.
        var coupon = await db.Coupons.FirstOrDefaultAsync(c => c.Code.Value == code.Value, ct);
        coupon?.RecordUse(paid.CustomerId, paid.OrderId, DateTime.Now);
    }
}
