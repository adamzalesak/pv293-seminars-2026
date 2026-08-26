namespace Kofi.DataAccessLayer.Models;

public class Subscription
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = "EUR";
    public int IntervalDays { get; set; }

    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Incomplete;
    public DateTime CreatedAt { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public DateTime? PausedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime? NextDeliveryAt { get; set; }
    public List<SubscriptionDelivery> Deliveries { get; set; } = new();
}
