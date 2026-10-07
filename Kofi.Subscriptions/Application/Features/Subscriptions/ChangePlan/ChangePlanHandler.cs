using Kofi.Catalog.Contracts;
using Kofi.Common.Abstractions;
using Kofi.Subscriptions.Application.Repositories;
using Kofi.Subscriptions.Domain.Subscriptions;

namespace Kofi.Subscriptions.Application.Features.Subscriptions.ChangePlan;

public class ChangePlanHandler(
    ISubscriptionRepository subscriptions,
    ICatalog products) : ICommandHandler<ChangePlanCommand>
{
    public async Task Handle(ChangePlanCommand command, CancellationToken ct)
    {
        var subscription = await subscriptions.GetById(command.Id)
                           ?? throw new NotFoundException("Subscription not found");

        var product = await products.GetProduct(command.ProductId, ct)
                      ?? throw new NotFoundException("Product not found");

        var offer = new OfferedProduct(product.Id, product.Name, new Price(product.Price, product.Currency), product.CanBeSubscribed);

        subscription.ChangePlan(offer, new Quantity(command.Quantity), command.IntervalDays);
    }
}
