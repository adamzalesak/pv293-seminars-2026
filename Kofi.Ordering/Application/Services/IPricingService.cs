using Kofi.Ordering.Domain.Orders;

namespace Kofi.Ordering.Application.Services;

public interface IPricingService
{
    decimal CalculateItemsTotal(IReadOnlyList<OrderItem> items);
    decimal CalculateShipping(decimal itemsTotal);
}
