using Kofi.Ordering.Domain.Orders;

namespace Kofi.Ordering.Application.Services;

public interface IPricingService
{
    decimal CalculateItemsTotal(IReadOnlyList<OrderItem> items);
    decimal CalculateShipping(decimal itemsTotal);
    decimal CalculateDiscount(decimal itemsTotal, int? discountPercent);
    decimal CalculateTotal(decimal itemsTotal, int? discountPercent);
}
