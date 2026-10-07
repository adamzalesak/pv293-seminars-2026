using Kofi.Common.Abstractions;
using Kofi.Common.Services;
using Kofi.Marketing.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Marketing.Application.Features.Coupons.GetMyCoupons;

// The referral codes a customer can pass on to friends.
public class GetMyCouponsHandler(IMarketingDbContext db, ICurrentUser user) : IQueryHandler<GetMyCouponsQuery, List<MyCoupon>>
{
    public async Task<List<MyCoupon>> Handle(GetMyCouponsQuery query, CancellationToken ct)
    {
        var now = DateTime.Now;

        return await db.Coupons.AsNoTracking()
            .Where(c => c.OwnerCustomerId != null && c.OwnerCustomerId == user.CustomerId)
            .OrderByDescending(c => c.IssuedAt)
            .Select(c => new MyCoupon
            {
                Code = c.Code.Value,
                DiscountPercent = c.Discount.Percent,
                ExpiresAt = c.ExpiresAt,
                IsValid = !c.IsInvalidated && c.ExpiresAt > now,
                UseCount = c.Uses.Count
            })
            .ToListAsync(ct);
    }
}
