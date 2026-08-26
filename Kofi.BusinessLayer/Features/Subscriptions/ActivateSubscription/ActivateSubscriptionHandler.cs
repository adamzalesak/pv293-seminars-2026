using Kofi.BusinessLayer.Abstractions;
using Kofi.BusinessLayer.Services;
using Kofi.DataAccessLayer.Models;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Features.Subscriptions.ActivateSubscription;

public class ActivateSubscriptionHandler(
    ISubscriptionRepository subscriptions,
    ICustomerRepository customers,
    IPaymentGatewayClient payments,
    ISubscriptionOrderService orders,
    IEmailService email) : ICommandHandler<ActivateSubscriptionCommand, ActivateSubscriptionResponse>
{
    public async Task<ActivateSubscriptionResponse> Handle(ActivateSubscriptionCommand command, CancellationToken ct)
    {
        var subscription = await subscriptions.GetById(command.Id) ?? throw new DomainException("Subscription not found");
        var now = DateTime.Now;

        var customer = await customers.GetById(subscription.CustomerId) ?? throw new DomainException("Customer not found");
        string? card = null;
        if (command.SetupId is { } setupId)
        {
            // The return URL only says the customer came back; the provider says
            // whether they authorised the card, and for which subscription.
            var setup = await payments.GetCardSetup(setupId);
            if (setup.Reference != subscription.Id.ToString())
                throw new DomainException("This card authorisation is for another subscription");
            card = setup.Status.ToString();
            // A reload of the return page, or a card the customer did not
            // authorise, changes nothing.
            if (subscription.Status != SubscriptionStatus.Incomplete || setup.Status != CardSetupStatus.Succeeded)
                return new ActivateSubscriptionResponse { Status = subscription.Status.ToString(), CardSetupStatus = card };
            customer.PaymentMethodToken = setup.PaymentMethodToken;
            customer.PaymentCardLabel = setup.CardLabel;
        }
        else
        {
            // The card the customer saved earlier already allows future charges.
            if (subscription.Status != SubscriptionStatus.Incomplete)
                throw new DomainException("Only incomplete subscriptions can be activated");
            if (customer.PaymentMethodToken == null)
                throw new DomainException("There is no saved card; authorise one first");
        }

        // The day the customer chose, or today if it has already passed.
        var firstDay = subscription.NextDeliveryAt!.Value < now.Date ? now.Date : subscription.NextDeliveryAt.Value;
        var first = new SubscriptionDelivery
        {
            Id = Guid.NewGuid(), SubscriptionId = subscription.Id, ProductId = subscription.ProductId,
            Quantity = subscription.Quantity, UnitPrice = subscription.UnitPrice, Currency = subscription.Currency,
            ScheduledFor = firstDay,
        };
        subscription.Deliveries.Add(first);
        subscription.Status = SubscriptionStatus.Active;
        subscription.ActivatedAt = now;
        subscription.NextDeliveryAt = firstDay;
        // Wanted today: the first order is created and charged straight away,
        // the same way PrepareDeliveryHandler does it. Later: process-due takes it.
        if (firstDay <= now)
        {
            var orderId = Guid.NewGuid();
            await orders.Create(subscription.CustomerId, orderId, now,
                [new(subscription.ProductId, subscription.ProductName, subscription.Quantity, subscription.UnitPrice, subscription.Currency)]);
            first.Status = DeliveryStatus.Ready;
            first.OrderId = orderId;
            subscription.NextDeliveryAt = firstDay.AddDays(subscription.IntervalDays);
            subscription.Deliveries.Add(new SubscriptionDelivery
            {
                Id = Guid.NewGuid(), SubscriptionId = subscription.Id, ProductId = subscription.ProductId,
                Quantity = subscription.Quantity, UnitPrice = subscription.UnitPrice, Currency = subscription.Currency,
                ScheduledFor = subscription.NextDeliveryAt.Value,
            });
        }
        await email.SendSubscriptionActivated(subscription);
        return new ActivateSubscriptionResponse { Status = subscription.Status.ToString(), CardSetupStatus = card };
    }
}
