using Kofi.BusinessLayer.Abstractions;
using Kofi.DataAccessLayer.Models;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Features.Subscriptions.ChangePlan;

public class ChangePlanHandler(ISubscriptionRepository subscriptions, IProductRepository products) : ICommandHandler<ChangePlanCommand>
{
    public async Task Handle(ChangePlanCommand command, CancellationToken ct)
    {
        var subscription = await subscriptions.GetById(command.Id)
            ?? throw new DomainException("Subscription not found");
        if (subscription.Status != SubscriptionStatus.Active && subscription.Status != SubscriptionStatus.Paused)
            throw new DomainException("Only active or paused subscriptions can change plan");
        var product = await products.GetById(command.ProductId)
            ?? throw new DomainException("Product not found");
        if (!product.IsActive)
            throw new DomainException("Product is no longer available");

        subscription.ProductId = product.Id;
        subscription.ProductName = product.Name;
        subscription.UnitPrice = product.Price;
        subscription.Quantity = command.Quantity;
        subscription.IntervalDays = command.IntervalDays;
        foreach (var delivery in subscription.Deliveries.Where(d => d.Status == DeliveryStatus.Scheduled))
        {
            delivery.ProductId = product.Id;
            delivery.UnitPrice = product.Price;
            delivery.Quantity = command.Quantity;
        }
    }
}
