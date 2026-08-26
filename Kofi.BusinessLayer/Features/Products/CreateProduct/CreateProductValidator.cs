using FluentValidation;

namespace Kofi.BusinessLayer.Features.Products.CreateProduct;

public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(c => c.Name).NotEmpty();
        RuleFor(c => c.Price).MustBeAPrice();
        RuleFor(c => c.StockQuantity).GreaterThanOrEqualTo(0);
    }
}
