namespace Kofi.Marketing.Application.Features.Coupons.CheckCoupon;

public record CouponCheck
{
    public required string Code { get; init; }
    public required int DiscountPercent { get; init; }
}
