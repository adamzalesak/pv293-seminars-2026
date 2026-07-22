using Kofi.DataAccessLayer.Models;

namespace Kofi.BusinessLayer.Services;

public interface IPricingService
{
    decimal CalculateItemsTotal(List<OrderItem> items);
    decimal CalculateShipping(decimal itemsTotal);
}
