using Kofi.BusinessLayer.Abstractions;
using Kofi.DataAccessLayer.Repositories;
using Kofi.Domain.Shared;
using Kofi.Domain.Subscriptions;

namespace Kofi.BusinessLayer.Features.Subscriptions.ChangePlan;

public class ChangePlanHandler(
    ISubscriptionRepository subscriptions,
    IProductRepository products) : ICommandHandler<ChangePlanCommand>
{
    public async Task Handle(ChangePlanCommand command, CancellationToken ct)
    {
        var subscription = await subscriptions.GetById(command.Id)
            ?? throw new NotFoundException("Subscription not found");
        var product = await products.GetById(command.ProductId)
            ?? throw new DomainException("Product not found");
        if (!product.IsActive)
        {
            throw new DomainException("Product is no longer available");
        }

        var plan = new Plan(product.Id, product.Name, new Quantity(command.Quantity), command.IntervalDays,
            new Price(product.Price, "EUR"));

        subscription.ChangePlan(plan);
    }
}
