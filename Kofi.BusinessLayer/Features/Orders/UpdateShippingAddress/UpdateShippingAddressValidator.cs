using FluentValidation;

namespace Kofi.BusinessLayer.Features.Orders.UpdateShippingAddress;

public class UpdateShippingAddressValidator : AbstractValidator<UpdateShippingAddressCommand>
{
    public UpdateShippingAddressValidator()
    {
        RuleFor(c => c.Street).NotEmpty();
        RuleFor(c => c.City).NotEmpty();
        RuleFor(c => c.Zip).NotEmpty();
    }
}
