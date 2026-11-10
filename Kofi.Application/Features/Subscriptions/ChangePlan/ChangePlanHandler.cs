using Kofi.Application.Abstractions;
using Kofi.Application.Repositories;
using Kofi.Domain.Shared;
using Kofi.Domain.Subscriptions;

namespace Kofi.Application.Features.Subscriptions.ChangePlan;

public class ChangePlanHandler(
    ISubscriptionRepository subscriptions,
    IProductRepository products) : ICommandHandler<ChangePlanCommand>
{
    public async Task Handle(ChangePlanCommand command, CancellationToken ct)
    {
        var subscription = await subscriptions.GetById(command.Id)
                           ?? throw new NotFoundException("Subscription not found");

        var product = await products.GetById(command.ProductId)
                      ?? throw new NotFoundException("Product not found");

        var offer = new OfferedProduct(product.Id, product.Name, product.Price, product.IsActive);

        subscription.ChangePlan(offer, new Quantity(command.Quantity), command.IntervalDays);
    }
}
