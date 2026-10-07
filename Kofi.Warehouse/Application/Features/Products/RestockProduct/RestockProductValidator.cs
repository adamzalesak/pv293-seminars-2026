using FluentValidation;

namespace Kofi.Warehouse.Application.Features.Products.RestockProduct;

public class RestockProductValidator : AbstractValidator<RestockProductCommand>
{
    public RestockProductValidator()
    {
        RuleFor(c => c.Quantity).GreaterThan(0);
    }
}
