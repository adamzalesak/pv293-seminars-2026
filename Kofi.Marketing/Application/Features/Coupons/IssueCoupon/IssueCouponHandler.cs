using Kofi.Common.Abstractions;
using Kofi.Marketing.Application.Persistence;
using Kofi.Marketing.Domain.Coupons;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Marketing.Application.Features.Coupons.IssueCoupon;

public class IssueCouponHandler(IMarketingDbContext db) : ICommandHandler<IssueCouponCommand, IssueCouponResponse>
{
    public async Task<IssueCouponResponse> Handle(IssueCouponCommand command, CancellationToken ct)
    {
        var coupon = Coupon.Issue(new CouponCode(command.Code), new Discount(command.DiscountPercent), command.ExpiresAt,
            DateTime.Now);

        if (await db.Coupons.AnyAsync(c => c.Code.Value == coupon.Code.Value, ct))
        {
            throw new DomainRuleException("A coupon with this code already exists.");
        }

        db.Coupons.Add(coupon);
        return new IssueCouponResponse { Id = coupon.Id, Code = coupon.Code.Value };
    }
}
