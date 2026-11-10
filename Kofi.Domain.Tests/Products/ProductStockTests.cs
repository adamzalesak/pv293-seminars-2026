using Kofi.Domain.Products;
using Kofi.Domain.Shared;
using Xunit;

namespace Kofi.Domain.Tests.Products;

public class ProductStockTests
{
    [Fact]
    public void Reservation_cannot_make_available_stock_negative()
    {
        var product = Product.Create(Guid.NewGuid(), "Coffee", "", new Price(12, "EUR"), 3);
        Assert.Throws<DomainRuleException>(() => product.Reserve(new Quantity(4)));
        Assert.Equal(3, product.StockQuantity);
        product.Reserve(new Quantity(2));
        product.Release(new Quantity(2));
        Assert.Equal(3, product.StockQuantity);
    }
}
