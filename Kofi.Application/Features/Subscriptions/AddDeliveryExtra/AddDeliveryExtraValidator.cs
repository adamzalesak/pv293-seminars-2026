using FluentValidation;

namespace Kofi.Application.Features.Subscriptions.AddDeliveryExtra;

public class AddDeliveryExtraValidator : AbstractValidator<AddDeliveryExtraCommand>
{
    public AddDeliveryExtraValidator()
    {
        RuleFor(c => c.ProductId).NotEmpty();
        RuleFor(c => c.Quantity).InclusiveBetween(1, 100);
    }
}
