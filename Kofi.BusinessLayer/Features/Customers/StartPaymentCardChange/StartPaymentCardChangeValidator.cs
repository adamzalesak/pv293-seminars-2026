using FluentValidation;

namespace Kofi.BusinessLayer.Features.Customers.StartPaymentCardChange;

public class StartPaymentCardChangeValidator : AbstractValidator<StartPaymentCardChangeCommand>
{
    public StartPaymentCardChangeValidator()
    {
        RuleFor(c => c.ReturnUrl).NotEmpty();
    }
}
