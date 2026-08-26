using Kofi.BusinessLayer.Abstractions;
using Kofi.BusinessLayer.Services;
using Kofi.DataAccessLayer.Models;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Features.Subscriptions.StartCardSetup;

// The customer authorises future charges on the provider's page. We learn the
// result only when we ask the provider ourselves, in ActivateSubscription.
public class StartCardSetupHandler(ISubscriptionRepository subscriptions, IPaymentGatewayClient payments)
    : ICommandHandler<StartCardSetupCommand, StartCardSetupResponse>
{
    public async Task<StartCardSetupResponse> Handle(StartCardSetupCommand command, CancellationToken ct)
    {
        var subscription = await subscriptions.GetById(command.Id) ?? throw new DomainException("Subscription not found");
        if (subscription.Status != SubscriptionStatus.Incomplete)
            throw new DomainException("Only an incomplete subscription needs a card");

        // Every attempt is a new setup; one the customer declined stays declined.
        var setup = await payments.CreateCardSetup(subscription.Id.ToString(), command.ReturnUrl, Guid.NewGuid().ToString());
        return new StartCardSetupResponse { SetupId = setup.SetupId, RedirectUrl = setup.RedirectUrl };
    }
}
