using Kofi.BusinessLayer.Abstractions;
using Kofi.BusinessLayer.Services;
using Kofi.DataAccessLayer.Repositories;
using Kofi.Domain.Shared;
using Kofi.Domain.Subscriptions;

namespace Kofi.BusinessLayer.Features.Subscriptions.Subscribe;

public class SubscribeHandler(
    ISubscriptionRepository subscriptions,
    ICustomerRepository customers,
    IProductRepository products,
    ICurrentUser user) : ICommandHandler<SubscribeCommand, SubscribeResponse>
{
    public async Task<SubscribeResponse> Handle(SubscribeCommand command, CancellationToken ct)
    {
        // The customer subscribing is the one signed in.
        var customer = await customers.GetById(user.CustomerId ?? Guid.Empty)
            ?? throw new DomainException("Only a signed-in customer can subscribe");
        var product = await products.GetById(command.ProductId)
            ?? throw new DomainException("Product not found");
        if (!product.IsActive)
        {
            throw new DomainException("Product is no longer available");
        }

        var plan = new Plan(product.Id, product.Name, new Quantity(command.Quantity), command.IntervalDays,
            new Price(product.Price, "EUR"));
        // Nothing is planned or charged until the customer authorises a card.
        var subscription = Subscription.Create(customer.Id, plan,
            firstDeliveryOn: command.FirstDeliveryOn.ToDateTime(TimeOnly.MinValue), now: DateTime.Now);
        subscriptions.Add(subscription);

        return new SubscribeResponse
        {
            Id = subscription.Id,
            Status = subscription.Status.ToString(),
        };
    }
}
