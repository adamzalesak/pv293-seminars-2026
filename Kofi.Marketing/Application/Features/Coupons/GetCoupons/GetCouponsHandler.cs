using Kofi.Common.Abstractions;
using Kofi.Marketing.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Marketing.Application.Features.Coupons.GetCoupons;

public class GetCouponsHandler(IMarketingDbContext db) : IQueryHandler<GetCouponsQuery, List<CouponSummary>>
{
    public async Task<List<CouponSummary>> Handle(GetCouponsQuery query, CancellationToken ct) =>
        await db.Coupons.AsNoTracking()
            .OrderByDescending(c => c.IssuedAt)
            .Select(c => new CouponSummary
            {
                Id = c.Id,
                Code = c.Code.Value,
                Reason = c.Reason.ToString(),
                OwnerCustomerId = c.OwnerCustomerId,
                DiscountPercent = c.Discount.Percent,
                IssuedAt = c.IssuedAt,
                ExpiresAt = c.ExpiresAt,
                IsInvalidated = c.IsInvalidated,
                UseCount = c.Uses.Count
            })
            .ToListAsync(ct);
}
