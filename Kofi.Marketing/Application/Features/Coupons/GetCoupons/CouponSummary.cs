namespace Kofi.Marketing.Application.Features.Coupons.GetCoupons;

public record CouponSummary
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Reason { get; init; }
    public Guid? OwnerCustomerId { get; init; }
    public required int DiscountPercent { get; init; }
    public required DateTime IssuedAt { get; init; }
    public required DateTime ExpiresAt { get; init; }
    public required bool IsInvalidated { get; init; }
    public required int UseCount { get; init; }
}
