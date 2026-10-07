using FluentValidation;

namespace Kofi.Catalog.Application.Features.Products.ChangeProductPrice;

public class ChangeProductPriceValidator : AbstractValidator<ChangeProductPriceCommand>
{
    public ChangeProductPriceValidator()
    {
        RuleFor(c => c.Price).MustBeAPrice();
    }
}
