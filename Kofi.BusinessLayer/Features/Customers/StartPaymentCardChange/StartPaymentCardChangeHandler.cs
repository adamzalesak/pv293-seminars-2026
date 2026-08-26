using Kofi.BusinessLayer.Abstractions;
using Kofi.BusinessLayer.Services;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Features.Customers.StartPaymentCardChange;

// The same card authorisation as for a new subscription, for the customer's
// card as a whole: every subscription is charged to the new one from now on.
public class StartPaymentCardChangeHandler(
    ICustomerRepository customers,
    IPaymentGatewayClient payments,
    ICurrentUser user) : ICommandHandler<StartPaymentCardChangeCommand, StartPaymentCardChangeResponse>
{
    public async Task<StartPaymentCardChangeResponse> Handle(StartPaymentCardChangeCommand command, CancellationToken ct)
    {
        var customer = await customers.GetById(user.CustomerId ?? Guid.Empty)
            ?? throw new DomainException("Only a signed-in customer can change their card");
        var setup = await payments.CreateCardSetup(PaymentCardReference.For(customer.Id), command.ReturnUrl, Guid.NewGuid().ToString());
        return new StartPaymentCardChangeResponse { SetupId = setup.SetupId, RedirectUrl = setup.RedirectUrl };
    }
}
