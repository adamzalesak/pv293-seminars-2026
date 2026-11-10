using FluentValidation;

namespace Kofi.Application.Features.Subscriptions.StartCardSetup;

public class StartCardSetupValidator : AbstractValidator<StartCardSetupCommand>
{
    public StartCardSetupValidator()
    {
        RuleFor(c => c.ReturnUrl).NotEmpty();
    }
}
