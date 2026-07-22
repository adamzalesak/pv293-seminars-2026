using Kofi.BusinessLayer.Dtos;
using Kofi.DataAccessLayer.Models;

namespace Kofi.BusinessLayer.Mappers;

// Mapping moved out of the endpoints and into the business layer, so that
// every service maps the same way.
public static class OrderMapper
{
    public static OrderDto ToDto(this Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            CustomerName = order.CustomerName,
            CustomerEmail = order.CustomerEmail,
            Street = order.Street,
            City = order.City,
            Zip = order.Zip,
            Status = order.Status.ToString(),
            // Total is left to the caller: it is not stored, and this mapper
            // has no way to reach PricingService.
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(i => i.ToDto()).ToList(),
        };
    }
}
