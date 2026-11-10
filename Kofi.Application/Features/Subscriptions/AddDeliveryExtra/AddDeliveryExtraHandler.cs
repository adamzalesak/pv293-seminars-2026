using Kofi.Application.Repositories;
using Kofi.Common.Abstractions;
using Kofi.Domain.Products;
using Kofi.Domain.Shared;
using Kofi.Domain.Subscriptions;

namespace Kofi.Application.Features.Subscriptions.AddDeliveryExtra;

public class AddDeliveryExtraHandler(ISubscriptionRepository subscriptions, IProductRepository products)
    : ICommandHandler<AddDeliveryExtraCommand>
{
    public async Task Handle(AddDeliveryExtraCommand command, CancellationToken ct)
    {
        var subscription = await subscriptions.GetById(command.SubscriptionId) ?? throw new NotFoundException("Subscription not found");

        var product = await products.GetById(command.ProductId) ?? throw new NotFoundException("Product not found");
        subscription.AddExtra(command.DeliveryId,
            new OfferedProduct(product.Id, product.Name, product.Price, product.Status == ProductStatus.Active),
            new Quantity(command.Quantity));
    }
}
