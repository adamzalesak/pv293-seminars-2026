using Kofi.BusinessLayer.Abstractions;
using Kofi.BusinessLayer.Services;
using Kofi.DataAccessLayer.Repositories;

namespace Kofi.BusinessLayer.Features.Customers.GetPaymentCard;

// The card every subscription of the signed-in customer is charged to.
public class GetPaymentCardHandler(ICustomerRepository customers, ICurrentUser user)
    : IQueryHandler<GetPaymentCardQuery, PaymentCard>
{
    public async Task<PaymentCard> Handle(GetPaymentCardQuery query, CancellationToken ct)
    {
        var customer = await customers.GetById(user.CustomerId ?? Guid.Empty)
            ?? throw new DomainException("Only a signed-in customer has a payment card");
        return new PaymentCard { Label = customer.PaymentMethodToken == null ? null : customer.PaymentCardLabel };
    }
}
