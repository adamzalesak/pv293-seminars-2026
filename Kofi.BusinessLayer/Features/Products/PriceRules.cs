using FluentValidation;

namespace Kofi.BusinessLayer.Features.Products;

public static class PriceRules
{
    // numeric(18,2) stores sixteen whole-number digits and two decimal places.
    private const decimal MaxStoredPrice = 9999999999999999.99m;

    // Money in KoFI has two decimal places, and so do the columns that store
    // it. A third one would be silently rounded away, so we refuse it here
    // instead of changing the price behind the caller's back.
    public static IRuleBuilderOptions<T, decimal> MustBeAPrice<T>(this IRuleBuilder<T, decimal> rule) =>
        rule.GreaterThan(0).LessThanOrEqualTo(MaxStoredPrice)
            .Must(price => decimal.Round(price, 2) == price)
            .WithMessage("'{PropertyName}' must have at most two decimal places.");
}
