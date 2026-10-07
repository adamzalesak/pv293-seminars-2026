namespace Kofi.Marketing.Application.Features.Coupons.UpdateCoupon;

public record UpdateCouponRequest
{
    public int DiscountPercent { get; init; }
    public DateTime ExpiresAt { get; init; }
}
