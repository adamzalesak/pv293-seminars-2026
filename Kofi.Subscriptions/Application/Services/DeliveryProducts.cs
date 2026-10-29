using Kofi.Catalog.Contracts;
using Kofi.Common.Abstractions;
using Kofi.Subscriptions.Domain.Subscriptions;

namespace Kofi.Subscriptions.Application.Services;

public class DeliveryProducts(ICatalog catalog) : IDeliveryProducts
{
    public async Task EnsureOnSale(Delivery delivery, CancellationToken ct)
    {
        foreach (var id in delivery.Extras.Select(e => e.ProductId).Prepend(delivery.ProductId).Distinct())
        {
            var product = await catalog.GetProduct(id, ct) ?? throw new NotFoundException("Product not found");
            if (!product.CanBeSold)
            {
                throw new DomainRuleException("A delivery product is no longer available");
            }
        }
    }
}
