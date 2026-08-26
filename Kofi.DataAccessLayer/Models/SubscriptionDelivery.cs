namespace Kofi.DataAccessLayer.Models;

public class SubscriptionDelivery
{
    public List<SubscriptionDeliveryExtra> Extras { get; set; } = new();
    public Guid Id { get; set; }
    public Guid SubscriptionId { get; set; }
    public Subscription Subscription { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = "EUR";
    public DateTime ScheduledFor { get; set; }
    public DeliveryStatus Status { get; set; } = DeliveryStatus.Scheduled;
    public Guid? OrderId { get; set; }
    public Order? Order { get; set; }
}
