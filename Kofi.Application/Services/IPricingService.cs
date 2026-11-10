using Kofi.Domain.Orders;

namespace Kofi.Application.Services;

public interface IPricingService
{
    decimal CalculateItemsTotal(IReadOnlyList<OrderItem> items);
    decimal CalculateShipping(decimal itemsTotal);
}
