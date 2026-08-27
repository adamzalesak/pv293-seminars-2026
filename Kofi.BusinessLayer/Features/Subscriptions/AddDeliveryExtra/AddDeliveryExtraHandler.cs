using Kofi.BusinessLayer.Abstractions;
using Kofi.DataAccessLayer.Repositories;
using Kofi.Domain.Shared;

namespace Kofi.BusinessLayer.Features.Subscriptions.AddDeliveryExtra;

public class AddDeliveryExtraHandler(ISubscriptionRepository subscriptions, IProductRepository products)
    : ICommandHandler<AddDeliveryExtraCommand>
{
    public async Task Handle(AddDeliveryExtraCommand command, CancellationToken ct)
    {
        var subscription = await subscriptions.GetById(command.SubscriptionId) ?? throw new NotFoundException("Subscription not found");
        var product = await products.GetById(command.ProductId) ?? throw new DomainException("Product not found");
        if (!product.IsActive) throw new DomainException("Product is no longer available");
        subscription.AddExtra(command.DeliveryId, product.Id, product.Name, new Quantity(command.Quantity),
            new Price(product.Price, "EUR"));
    }
}
