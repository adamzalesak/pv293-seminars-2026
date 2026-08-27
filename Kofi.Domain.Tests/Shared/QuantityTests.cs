using Kofi.Domain.Shared;
using Kofi.Domain;
using Xunit;

namespace Kofi.Domain.Tests.Shared;

// A quantity that cannot exist cannot be made.
public class QuantityTests
{
    [Fact]
    public void Two_quantities_of_the_same_size_are_the_same_value()
    {
        Assert.Equal(new Quantity(2), new Quantity(2));
    }

    [Fact]
    public void Quantities_add_up()
    {
        Assert.Equal(new Quantity(5), new Quantity(2) + new Quantity(3));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void A_quantity_is_at_least_one(int value)
    {
        Assert.Throws<DomainRuleException>(() => new Quantity(value));
    }
}
