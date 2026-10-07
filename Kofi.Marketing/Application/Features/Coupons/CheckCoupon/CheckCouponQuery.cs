using Kofi.Common.Abstractions;

namespace Kofi.Marketing.Application.Features.Coupons.CheckCoupon;

public record CheckCouponQuery : IQuery<CouponCheck>
{
    public string Code { get; init; } = "";
}
