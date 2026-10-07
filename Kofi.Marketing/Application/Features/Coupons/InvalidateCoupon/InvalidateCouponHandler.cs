using Kofi.Common.Abstractions;
using Kofi.Marketing.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Marketing.Application.Features.Coupons.InvalidateCoupon;

public class InvalidateCouponHandler(IMarketingDbContext db) : ICommandHandler<InvalidateCouponCommand>
{
    public async Task Handle(InvalidateCouponCommand command, CancellationToken ct)
    {
        var coupon = await db.Coupons.FirstOrDefaultAsync(c => c.Id == command.Id, ct) ?? throw new NotFoundException("Coupon not found");
        coupon.Invalidate();
    }
}
