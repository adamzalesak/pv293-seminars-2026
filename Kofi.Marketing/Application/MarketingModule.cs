using Kofi.Common.Abstractions;
using Kofi.Marketing.Application.Persistence;
using Kofi.Marketing.Contracts;
using Kofi.Marketing.Domain.Coupons;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Marketing.Application;

public class MarketingModule(IMarketingDbContext db) : IMarketing
{
    public async Task<CouponDiscount> ValidateCoupon(string code, Guid customerId, CancellationToken ct = default)
    {
        var coupon = await Find(code, ct);
        coupon.EnsureUsableBy(customerId, DateTime.Now);
        return new CouponDiscount(coupon.Code.Value, coupon.Discount.Percent);
    }

    private async Task<Coupon> Find(string code, CancellationToken ct)
    {
        var wanted = new CouponCode(code);
        return await db.Coupons.FirstOrDefaultAsync(c => c.Code.Value == wanted.Value, ct) ??
               throw new NotFoundException("Coupon not found");
    }
}
