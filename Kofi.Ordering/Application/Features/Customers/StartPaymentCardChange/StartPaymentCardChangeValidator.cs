using FluentValidation;

namespace Kofi.Ordering.Application.Features.Customers.StartPaymentCardChange;

public class StartPaymentCardChangeValidator : AbstractValidator<StartPaymentCardChangeCommand>
{
    public StartPaymentCardChangeValidator()
    {
        RuleFor(c => c.ReturnUrl).NotEmpty();
    }
}
