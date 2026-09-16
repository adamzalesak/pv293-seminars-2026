using FluentValidation;

namespace Kofi.Application.Features.Products.RestockProduct;

public class RestockProductValidator : AbstractValidator<RestockProductCommand>
{
    public RestockProductValidator()
    {
        RuleFor(c => c.Quantity).GreaterThan(0);
    }
}
