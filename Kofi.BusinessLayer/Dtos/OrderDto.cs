namespace Kofi.BusinessLayer.Dtos;

// One DTO per entity, used for reading and for writing. Creating an order
// posts the same shape the detail returns, so half the fields are ignored on
// the way in and half are left empty on the way out. Adding a field for one
// screen adds it to all of them.
public class OrderDto
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = "";
    public string CustomerEmail { get; set; } = "";
    public string Street { get; set; } = "";
    public string City { get; set; } = "";
    public string Zip { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public decimal ItemsTotal { get; set; }
    public decimal Shipping { get; set; }
    public decimal Total { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}
