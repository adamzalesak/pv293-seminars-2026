using Kofi.Domain.Shared;

namespace Kofi.Domain.Subscriptions;

public record OfferedProduct(Guid Id, string Name, Price Price, bool IsOnSale)
{
    public void EnsureOnSale()
    {
        if (!IsOnSale)
        {
            throw new DomainRuleException("Product is no longer available.");
        }
    }
}
