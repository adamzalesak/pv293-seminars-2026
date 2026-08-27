using Kofi.Domain.Shared;
using Kofi.Domain;
using Xunit;

namespace Kofi.Domain.Tests.Shared;

// A price that cannot exist cannot be made.
public class PriceTests
{
    [Fact]
    public void Two_amounts_in_the_same_currency_are_the_same_value()
    {
        Assert.Equal(new Price(249m, "CZK"), new Price(249m, "CZK"));
    }

    [Fact]
    public void The_same_amount_in_another_currency_is_a_different_value()
    {
        Assert.NotEqual(new Price(249m, "CZK"), new Price(249m, "EUR"));
    }

    [Fact]
    public void Amounts_in_one_currency_add_up()
    {
        Assert.Equal(new Price(15m, "EUR"), new Price(10m, "EUR") + new Price(5m, "EUR"));
    }

    [Fact]
    public void Adding_two_currencies_is_rejected()
    {
        Assert.Throws<DomainRuleException>(
            () => new Price(10m, "EUR") + new Price(10m, "CZK"));
    }

    [Fact]
    public void A_price_multiplies_by_a_quantity()
    {
        Assert.Equal(new Price(25m, "EUR"), new Price(12.50m, "EUR") * new Quantity(2));
    }

    [Fact]
    public void A_currency_is_required()
    {
        Assert.Throws<DomainRuleException>(() => new Price(10m, ""));
    }

    [Fact]
    public void A_price_cannot_be_negative()
    {
        Assert.Throws<DomainRuleException>(() => new Price(-0.01m, "EUR"));
    }

    [Fact]
    public void A_free_item_costs_zero()
    {
        Assert.Equal(0m, new Price(0m, "EUR").Amount);
    }

    [Fact]
    public void A_price_has_at_most_two_decimal_places()
    {
        Assert.Throws<DomainRuleException>(() => new Price(9.999m, "EUR"));
    }

    [Fact]
    public void A_price_fits_the_column_that_stores_it()
    {
        Assert.Throws<DomainRuleException>(() => new Price(Price.MaxStoredAmount + 0.01m, "EUR"));
    }
}
