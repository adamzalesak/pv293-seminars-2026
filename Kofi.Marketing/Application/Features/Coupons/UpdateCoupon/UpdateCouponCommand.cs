using Kofi.Common.Abstractions;

namespace Kofi.Marketing.Application.Features.Coupons.UpdateCoupon;

public record UpdateCouponCommand : ICommand
{
    public Guid Id { get; init; }
    public int DiscountPercent { get; init; }
    public DateTime ExpiresAt { get; init; }
}
