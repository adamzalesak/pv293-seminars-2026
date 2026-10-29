namespace Kofi.Ordering.Contracts;

public interface IOrdering
{
    Task<CustomerInfo?> GetCustomer(Guid id, CancellationToken ct = default);
    Task<IReadOnlyDictionary<Guid, string>> GetCustomerNames(IReadOnlyList<Guid> ids, CancellationToken ct = default);
    Task SavePaymentCard(Guid customerId, string token, string? label, CancellationToken ct = default);
    Task<IReadOnlyDictionary<Guid, string>> GetOrderStatuses(IReadOnlyList<Guid> ids, CancellationToken ct = default);
}
