using FluentValidation;

namespace Kofi.Application.Features.Products.UpdateProduct;

public class UpdateProductValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductValidator()
    {
        RuleFor(c => c.Name).NotEmpty();
    }
}
