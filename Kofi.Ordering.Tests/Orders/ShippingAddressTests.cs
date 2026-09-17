using Kofi.Common.Domain;
using Kofi.Ordering.Domain.Shared;
using Xunit;

namespace Kofi.Ordering.Tests.Orders;

public class ShippingAddressTests
{
    [Theory]
    [InlineData("60200")]
    [InlineData("602 00")]
    public void Postal_code_has_one_stored_spelling(string zip) =>
        Assert.Equal("60200", new ShippingAddress("Kounicova 12", "Brno", zip).Zip);

    [Theory]
    [InlineData("Kounicova", "Brno", "60200")]
    [InlineData("Kounicova 12", "", "60200")]
    [InlineData("Kounicova 12", "Brno", "6020")]
    public void Incomplete_addresses_cannot_be_created(string street, string city, string zip) =>
        Assert.Throws<DomainRuleException>(() => new ShippingAddress(street, city, zip));
}
