using Kofi.Common.Abstractions;
using Kofi.Marketing.Application.Persistence;
using Kofi.Marketing.Domain.Coupons;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Marketing.Application.Features.Coupons.UpdateCoupon;

public class UpdateCouponHandler(IMarketingDbContext db) : ICommandHandler<UpdateCouponCommand>
{
    public async Task Handle(UpdateCouponCommand command, CancellationToken ct)
    {
        var coupon = await db.Coupons.FirstOrDefaultAsync(c => c.Id == command.Id, ct) ?? throw new NotFoundException("Coupon not found");
        coupon.Update(new Discount(command.DiscountPercent), command.ExpiresAt, DateTime.Now);
    }
}
