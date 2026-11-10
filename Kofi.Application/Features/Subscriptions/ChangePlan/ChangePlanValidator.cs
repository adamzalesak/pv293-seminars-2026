using FluentValidation;

namespace Kofi.Application.Features.Subscriptions.ChangePlan;

public class ChangePlanValidator : AbstractValidator<ChangePlanCommand>
{
    public ChangePlanValidator()
    {
        RuleFor(c => c.ProductId).NotEmpty();
        RuleFor(c => c.Quantity).InclusiveBetween(1, 100);
        RuleFor(c => c.IntervalDays).InclusiveBetween(1, 365);
    }
}
