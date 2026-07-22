using Kofi.BusinessLayer.Dtos;
using Kofi.DataAccessLayer.Models;

namespace Kofi.BusinessLayer.Mappers;

public static class OrderItemMapper
{
    public static OrderItemDto ToDto(this OrderItem item)
    {
        return new OrderItemDto
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            UnitPrice = item.UnitPrice,
            Quantity = item.Quantity,
        };
    }
}
