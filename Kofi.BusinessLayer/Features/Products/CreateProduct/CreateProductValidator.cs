using FluentValidation;

namespace Kofi.BusinessLayer.Features.Products.CreateProduct;

public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(c => c.Name).NotEmpty();
        RuleFor(c => c.Price).GreaterThan(0);
        RuleFor(c => c.StockQuantity).GreaterThanOrEqualTo(0);
    }
}
