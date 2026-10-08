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
        var coupon = await db.Coupons.FirstOrDefaultAsync(c => c.Code.Value == code.Value, ct) ??
                     throw new NotFoundException("Coupon not found");
        coupon.Redeem(paid.CustomerId, paid.OrderId, DateTime.Now);
    }
}
