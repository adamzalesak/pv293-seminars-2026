using Kofi.Domain.Products;
using Kofi.Domain.Shared;
using Xunit;

namespace Kofi.Domain.Tests.Products;

public class ProductLifecycleTests
{
    [Fact]
    public void A_new_product_is_prepared_before_it_is_published()
    {
        var product = Product.Create(Guid.NewGuid(), "Coffee", "", new Price(12, "EUR"), 5);
        Assert.Equal(ProductStatus.Draft, product.Status);
        product.Activate();
        product.Discontinue();
        Assert.Equal(ProductStatus.Discontinued, product.Status);
        product.Deactivate();
        Assert.Equal(ProductStatus.Inactive, product.Status);
    }
}
