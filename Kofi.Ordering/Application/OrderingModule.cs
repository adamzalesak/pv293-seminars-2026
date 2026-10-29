using Kofi.Common.Abstractions;
using Kofi.Ordering.Application.Persistence;
using Kofi.Ordering.Application.Repositories;
using Kofi.Ordering.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Kofi.Ordering.Application;

public class OrderingModule(IOrderingDbContext db, ICustomerRepository customers) : IOrdering
{
    public Task<CustomerInfo?> GetCustomer(Guid id, CancellationToken ct = default) => db.Customers.AsNoTracking()
        .Where(c => c.Id == id)
        .Select(c => new CustomerInfo(c.Id, c.Name, c.Email.Value, c.PaymentMethodToken != null, c.PaymentCardLabel))
        .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyDictionary<Guid, string>> GetCustomerNames(IReadOnlyList<Guid> ids, CancellationToken ct = default) =>
        await db.Customers.AsNoTracking().Where(c => ids.Contains(c.Id)).ToDictionaryAsync(c => c.Id, c => c.Name, ct);

    public async Task SavePaymentCard(Guid customerId, string token, string? label, CancellationToken ct = default)
    {
        var customer = await customers.GetById(customerId) ?? throw new NotFoundException("Customer not found");

        customer.SavePaymentCard(token, label);
    }

    public async Task<IReadOnlyDictionary<Guid, string>> GetOrderStatuses(IReadOnlyList<Guid> ids, CancellationToken ct = default) =>
        await db.Orders.AsNoTracking().Where(o => ids.Contains(o.Id)).ToDictionaryAsync(o => o.Id, o => o.Status.ToString(), ct);
}
