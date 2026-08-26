namespace Kofi.DataAccessLayer.Models;

public class SubscriptionDeliveryExtra
{
    public Guid DeliveryId { get; set; }
    public SubscriptionDelivery Delivery { get; set; } = null!;
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = "EUR";
}
