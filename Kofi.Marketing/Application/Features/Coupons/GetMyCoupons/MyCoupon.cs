namespace Kofi.Marketing.Application.Features.Coupons.GetMyCoupons;

public record MyCoupon
{
    public required string Code { get; init; }
    public required int DiscountPercent { get; init; }
    public required DateTime ExpiresAt { get; init; }
    public required bool IsValid { get; init; }
    public required int UseCount { get; init; }
}
