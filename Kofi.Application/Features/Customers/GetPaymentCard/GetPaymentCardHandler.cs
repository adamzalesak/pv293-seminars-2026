using Kofi.Application.Abstractions;
using Kofi.Application.Persistence;
using Kofi.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Application.Features.Customers.GetPaymentCard;

public class GetPaymentCardHandler(IApplicationDbContext db, ICurrentUser user) : IQueryHandler<GetPaymentCardQuery, PaymentCard>
{
    public async Task<PaymentCard> Handle(GetPaymentCardQuery query, CancellationToken ct)
    {
        return await db.Customers.AsNoTracking()
                   .Where(c => c.Id == user.CustomerId)
                   .Select(c => new PaymentCard { Label = c.PaymentMethodToken == null ? null : c.PaymentCardLabel })
                   .FirstOrDefaultAsync(ct)
               ?? throw new NotFoundException("Customer not found");
    }
}
