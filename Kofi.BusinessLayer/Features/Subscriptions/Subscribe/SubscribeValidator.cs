using FluentValidation;

namespace Kofi.BusinessLayer.Features.Subscriptions.Subscribe;

public class SubscribeValidator : AbstractValidator<SubscribeCommand>
{
    public SubscribeValidator()
    {
        RuleFor(c => c.ProductId).NotEmpty();
        RuleFor(c => c.Quantity).InclusiveBetween(1, 100);
        RuleFor(c => c.IntervalDays).InclusiveBetween(1, 365);
        RuleFor(c => c.FirstDeliveryOn)
            .Must(day => day >= Today() && day <= Today().AddDays(90))
            .WithMessage("The first delivery must be between today and 90 days from now.");
    }

    private static DateOnly Today() => DateOnly.FromDateTime(DateTime.Now);
}
