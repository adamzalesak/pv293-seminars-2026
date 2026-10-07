namespace Kofi.Marketing.Application.Features.Coupons.IssueCoupon;

public record IssueCouponResponse
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
}
