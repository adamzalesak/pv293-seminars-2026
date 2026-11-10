using Kofi.Application.Abstractions;
using Kofi.Application.Repositories;
using Kofi.Application.Services;
using Kofi.Domain;
using Kofi.Domain.Subscriptions;

namespace Kofi.Application.Features.Subscriptions.ActivateSubscription;

public class ActivateSubscriptionHandler(
    ISubscriptionRepository subscriptions,
    ICustomerRepository customers,
    IPaymentGatewayClient payments,
    ISubscriptionOrderService orders) : ICommandHandler<ActivateSubscriptionCommand, ActivateSubscriptionResponse>
{
    public async Task<ActivateSubscriptionResponse> Handle(ActivateSubscriptionCommand command, CancellationToken ct)
    {
        var subscription = await subscriptions.GetById(command.Id) ?? throw new NotFoundException("Subscription not found");

        var now = DateTime.Now;

        var customer = await customers.GetById(subscription.CustomerId) ?? throw new NotFoundException("Customer not found");
        CardSetupStatus? card = null;

        if (command.SetupId is { } setupId)
        {
            // The return URL only says the customer came back; the provider says
            // whether they authorised the card, and for which subscription.
            var setup = await payments.GetCardSetup(setupId);
            if (setup.Reference != subscription.Id.ToString())
            {
                throw new DomainRuleException("This card authorisation is for another subscription");
            }

            card = setup.Status;

            // A reload of the return page, or a card the customer did not
            // authorise, changes nothing.
            if (subscription.Status != SubscriptionStatus.Incomplete || setup.Status != CardSetupStatus.Succeeded)
            {
                return Result(subscription, card);
            }

            customer.SavePaymentCard(setup.PaymentMethodToken!, setup.CardLabel);
        }
        else if (customer.PaymentMethodToken == null)
        {
            // Without a new authorisation only a card saved earlier can be charged.
            throw new DomainRuleException("There is no saved card; authorise one first");
        }

        subscription.Activate(now);

        if (subscription.PrepareIfDue(Guid.NewGuid(), now) is { } prepared)
        {
            await orders.Create(subscription.CustomerId, now, prepared);
        }

        return Result(subscription, card);
    }

    private static ActivateSubscriptionResponse Result(Subscription subscription, CardSetupStatus? card) =>
        new() { Status = subscription.Status.ToString(), CardSetupStatus = card?.ToString() };
}
