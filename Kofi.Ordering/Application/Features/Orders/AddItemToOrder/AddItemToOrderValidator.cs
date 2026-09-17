using FluentValidation;

namespace Kofi.Ordering.Application.Features.Orders.AddItemToOrder;

public class AddItemToOrderValidator : AbstractValidator<AddItemToOrderCommand>
{
    public AddItemToOrderValidator()
    {
        RuleFor(c => c.Quantity).InclusiveBetween(1, 100);
    }
}
