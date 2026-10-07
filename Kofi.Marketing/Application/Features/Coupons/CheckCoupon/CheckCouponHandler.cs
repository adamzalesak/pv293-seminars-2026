using Kofi.Common.Abstractions;
using Kofi.Common.Services;
using Kofi.Marketing.Application.Persistence;
using Kofi.Marketing.Domain.Coupons;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Marketing.Application.Features.Coupons.CheckCoupon;

// Lets the checkout show the discount before the order exists. Ordering
// checks the code again when the order is placed.
public class CheckCouponHandler(IMarketingDbContext db, ICurrentUser user) : IQueryHandler<CheckCouponQuery, CouponCheck>
{
    public async Task<CouponCheck> Handle(CheckCouponQuery query, CancellationToken ct)
    {
        var code = new CouponCode(query.Code);
        var coupon = await db.Coupons.AsNoTracking().FirstOrDefaultAsync(c => c.Code.Value == code.Value, ct) ??
                     throw new NotFoundException("Coupon not found");

        coupon.EnsureUsableBy(user.CustomerId ?? Guid.Empty, DateTime.Now);
        return new CouponCheck { Code = coupon.Code.Value, DiscountPercent = coupon.Discount.Percent };
    }
}
