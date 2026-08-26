using Kofi.BusinessLayer.Abstractions;
using Kofi.BusinessLayer.Services;
using Kofi.DataAccessLayer.Models;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Features.Subscriptions.Subscribe;

// The subscription starts incomplete: nothing is planned or charged until
// the customer authorises their card with the payment provider.
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
            throw new DomainException("Product is no longer available");

        var subscription = new Subscription
        {
            Id = Guid.NewGuid(), CustomerId = customer.Id, ProductId = product.Id,
            ProductName = product.Name, UnitPrice = product.Price, Quantity = command.Quantity,
            IntervalDays = command.IntervalDays, CreatedAt = DateTime.Now,
            // Until activation, the day the customer wants the first delivery.
            NextDeliveryAt = command.FirstDeliveryOn.ToDateTime(TimeOnly.MinValue),
        };
        subscriptions.Add(subscription);
        return new SubscribeResponse { Id = subscription.Id, Status = subscription.Status.ToString() };
    }
}
