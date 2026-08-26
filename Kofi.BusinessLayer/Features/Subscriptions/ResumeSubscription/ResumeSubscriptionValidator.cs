using FluentValidation;

namespace Kofi.BusinessLayer.Features.Subscriptions.ResumeSubscription;

public class ResumeSubscriptionValidator : AbstractValidator<ResumeSubscriptionCommand>
{
    public ResumeSubscriptionValidator()
    {
        RuleFor(c => c.NextDeliveryOn)
            .Must(day => day >= Today() && day <= Today().AddDays(90))
            .WithMessage("The next delivery must be between today and 90 days from now.");
    }

    private static DateOnly Today() => DateOnly.FromDateTime(DateTime.Now);
}
