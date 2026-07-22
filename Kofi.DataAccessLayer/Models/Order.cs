namespace Kofi.DataAccessLayer.Models;

public class Order
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public string CustomerName { get; set; } = "";
    public string CustomerEmail { get; set; } = "";
    public string Street { get; set; } = "";
    public string City { get; set; } = "";
    public string Zip { get; set; } = "";
    public OrderStatus Status { get; set; } = OrderStatus.New;
    public Guid? PaymentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItem> Items { get; set; } = new();
}
