using Kofi.BusinessLayer.Abstractions;
using Kofi.DataAccessLayer.Repositories;
using Kofi.DataAccessLayer.Models;
namespace Kofi.BusinessLayer.Features.Subscriptions.AddDeliveryExtra;

public class AddDeliveryExtraHandler(ISubscriptionRepository subscriptions, IProductRepository products)
    : ICommandHandler<AddDeliveryExtraCommand>
{
    public async Task Handle(AddDeliveryExtraCommand command, CancellationToken ct)
    {
        var subscription = await subscriptions.GetById(command.SubscriptionId) ?? throw new DomainException("Subscription not found");
        var product = await products.GetById(command.ProductId) ?? throw new DomainException("Product not found");
        if (!product.IsActive) throw new DomainException("Product is no longer available");
        if (subscription.Status is not (SubscriptionStatus.Active or SubscriptionStatus.Paused))
            throw new DomainException("Only a running subscription can change a delivery");
        var delivery = subscription.Deliveries.SingleOrDefault(d => d.Id == command.DeliveryId)
            ?? throw new DomainException("Delivery not found");
        if (delivery.Status != DeliveryStatus.Scheduled) throw new DomainException("Only a planned delivery can be changed");
        var existing = delivery.Extras.SingleOrDefault(e => e.ProductId == product.Id);
        if ((existing?.Quantity ?? 0) + command.Quantity > 100) throw new DomainException("An extra cannot exceed one hundred units");
        if (existing != null) existing.Quantity += command.Quantity;
        else delivery.Extras.Add(new SubscriptionDeliveryExtra
        {
            DeliveryId = delivery.Id, ProductId = product.Id, ProductName = product.Name,
            Quantity = command.Quantity, UnitPrice = product.Price, Currency = "EUR",
        });
    }
}
