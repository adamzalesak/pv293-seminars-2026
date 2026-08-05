using FluentValidation;

namespace Kofi.BusinessLayer.Features.Products.ChangeProductPrice;

public class ChangeProductPriceValidator : AbstractValidator<ChangeProductPriceCommand>
{
    public ChangeProductPriceValidator()
    {
        RuleFor(c => c.Price).GreaterThan(0);
    }
}
