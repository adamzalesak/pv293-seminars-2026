using Kofi.Application.Abstractions;
using Kofi.Application.Repositories;
using Kofi.Application.Services;
using Kofi.Domain;
using Kofi.Domain.Shared;
using Kofi.Domain.Subscriptions;

namespace Kofi.Application.Features.Subscriptions.Subscribe;

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
                       ?? throw new DomainRuleException("Only a signed-in customer can subscribe");
        var product = await products.GetById(command.ProductId)
                      ?? throw new NotFoundException("Product not found");

        var offer = new OfferedProduct(product.Id, product.Name, product.Price, product.IsActive);

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
