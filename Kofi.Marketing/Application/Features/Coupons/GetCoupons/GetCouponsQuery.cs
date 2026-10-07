using Kofi.Common.Abstractions;

namespace Kofi.Marketing.Application.Features.Coupons.GetCoupons;

public record GetCouponsQuery : IQuery<List<CouponSummary>>;
