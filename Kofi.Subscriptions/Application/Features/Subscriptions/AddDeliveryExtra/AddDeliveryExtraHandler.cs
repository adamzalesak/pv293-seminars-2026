using Kofi.Catalog.Contracts;
using Kofi.Common.Abstractions;
using Kofi.Subscriptions.Application.Repositories;
using Kofi.Subscriptions.Domain.Subscriptions;

namespace Kofi.Subscriptions.Application.Features.Subscriptions.AddDeliveryExtra;

public class AddDeliveryExtraHandler(ISubscriptionRepository subscriptions, ICatalog products)
    : ICommandHandler<AddDeliveryExtraCommand>
{
    public async Task Handle(AddDeliveryExtraCommand command, CancellationToken ct)
    {
        var subscription = await subscriptions.GetById(command.SubscriptionId) ?? throw new NotFoundException("Subscription not found");

        var product = await products.GetProduct(command.ProductId, ct) ?? throw new NotFoundException("Product not found");
        subscription.AddExtra(command.DeliveryId,
            new OfferedProduct(product.Id, product.Name, new Price(product.Price, product.Currency), product.CanBeSubscribed),
            new Quantity(command.Quantity));
    }
}
