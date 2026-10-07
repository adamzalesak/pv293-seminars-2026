using Kofi.Common.Abstractions;

namespace Kofi.Marketing.Application.Features.Coupons.InvalidateCoupon;

public record InvalidateCouponCommand : ICommand
{
    public Guid Id { get; init; }
}
