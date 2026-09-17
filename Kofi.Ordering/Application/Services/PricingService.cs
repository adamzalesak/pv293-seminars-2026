using Kofi.Ordering.Domain.Orders;

namespace Kofi.Ordering.Application.Services;

public class PricingService : IPricingService
{
    public const decimal ShippingFee = 4.99m;
    public const decimal FreeShippingFrom = 50m;

    public decimal CalculateItemsTotal(IReadOnlyList<OrderItem> items)
    {
        decimal total = 0;

        foreach (var item in items)
        {
            total = total + item.UnitPrice.Amount * item.Quantity.Value;
        }

        return total;
    }

    public decimal CalculateShipping(decimal itemsTotal)
    {
        if (itemsTotal >= FreeShippingFrom)
        {
            return 0;
        }

        return ShippingFee;
    }
}
