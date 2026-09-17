using FluentValidation;

namespace Kofi.Ordering.Application.Features.Customers.RegisterCustomer;

public class RegisterCustomerValidator : AbstractValidator<RegisterCustomerCommand>
{
    public RegisterCustomerValidator()
    {
        RuleFor(c => c.Name).NotEmpty();
        RuleFor(c => c.Email).NotEmpty().EmailAddress();
    }
}
