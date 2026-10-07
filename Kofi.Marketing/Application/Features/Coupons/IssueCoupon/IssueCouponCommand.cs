using Kofi.Common.Abstractions;

namespace Kofi.Marketing.Application.Features.Coupons.IssueCoupon;

public record IssueCouponCommand : ICommand<IssueCouponResponse>
{
    public string Code { get; init; } = "";
    public int DiscountPercent { get; init; }
    public DateTime ExpiresAt { get; init; }
}
