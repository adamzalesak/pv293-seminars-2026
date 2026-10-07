using Kofi.Catalog.Contracts;
using Kofi.Common.Abstractions;
using Kofi.Common.Services;
using Kofi.Ordering.Contracts;
using Kofi.Subscriptions.Application.Repositories;
using Kofi.Subscriptions.Domain.Subscriptions;

namespace Kofi.Subscriptions.Application.Features.Subscriptions.Subscribe;

public class SubscribeHandler(
    ISubscriptionRepository subscriptions,
    IOrdering ordering,
    ICatalog products,
    ICurrentUser user) : ICommandHandler<SubscribeCommand, SubscribeResponse>
{
    public async Task<SubscribeResponse> Handle(SubscribeCommand command, CancellationToken ct)
    {
        // The customer subscribing is the one signed in.
        var customer = await ordering.GetCustomer(user.CustomerId ?? Guid.Empty, ct)
                       ?? throw new DomainRuleException("Only a signed-in customer can subscribe");
        var product = await products.GetProduct(command.ProductId, ct)
                      ?? throw new NotFoundException("Product not found");

        var offer = new OfferedProduct(product.Id, product.Name, new Price(product.Price, product.Currency), product.CanBeSubscribed);

        // Nothing is planned or charged until the customer authorises a card.
        var subscription = Subscription.Create(customer.Id, offer, new Quantity(command.Quantity), command.IntervalDays,
            firstDeliveryOn: command.FirstDeliveryOn.ToDateTime(TimeOnly.MinValue), now: DateTime.Now);
        subscriptions.Add(subscription);

        return new SubscribeResponse
        {
            Id = subscription.Id,
            Status = subscription.Status.ToString(),
        };
    }
}
