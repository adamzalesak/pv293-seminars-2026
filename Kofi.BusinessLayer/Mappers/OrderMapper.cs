using Kofi.BusinessLayer.Dtos;
using Kofi.DataAccessLayer.Models;

namespace Kofi.BusinessLayer.Mappers;

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
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(i => i.ToDto()).ToList(),
        };
    }
}
