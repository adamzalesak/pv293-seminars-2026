using FluentValidation;

namespace Kofi.Marketing.Application.Features.Coupons.IssueCoupon;

public class IssueCouponValidator : AbstractValidator<IssueCouponCommand>
{
    public IssueCouponValidator()
    {
        RuleFor(c => c.Code).NotEmpty();
    }
}
