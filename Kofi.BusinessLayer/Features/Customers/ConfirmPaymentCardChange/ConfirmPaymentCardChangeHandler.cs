using Kofi.BusinessLayer.Abstractions;
using Kofi.BusinessLayer.Services;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Features.Customers.ConfirmPaymentCardChange;

// The provider sends the customer back; we ask it what was authorised and for
// whom, and only then replace the saved card. A declined setup keeps the old one.
public class ConfirmPaymentCardChangeHandler(ICustomerRepository customers, IPaymentGatewayClient payments)
    : ICommandHandler<ConfirmPaymentCardChangeCommand, ConfirmPaymentCardChangeResponse>
{
    public async Task<ConfirmPaymentCardChangeResponse> Handle(ConfirmPaymentCardChangeCommand command, CancellationToken ct)
    {
        var setup = await payments.GetCardSetup(command.SetupId);
        var customerId = PaymentCardReference.CustomerOf(setup.Reference)
            ?? throw new DomainException("This card authorisation is not a card change");
        var customer = await customers.GetById(customerId) ?? throw new DomainException("Customer not found");
        if (setup.Status == CardSetupStatus.Succeeded)
        {
            customer.PaymentMethodToken = setup.PaymentMethodToken;
            customer.PaymentCardLabel = setup.CardLabel;
        }
        return new ConfirmPaymentCardChangeResponse
        {
            CardSetupStatus = setup.Status.ToString(),
            Label = customer.PaymentCardLabel,
        };
    }
}
