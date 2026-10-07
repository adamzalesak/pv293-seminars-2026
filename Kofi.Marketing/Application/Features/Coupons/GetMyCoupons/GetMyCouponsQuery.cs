using Kofi.Common.Abstractions;

namespace Kofi.Marketing.Application.Features.Coupons.GetMyCoupons;

public record GetMyCouponsQuery : IQuery<List<MyCoupon>>;
