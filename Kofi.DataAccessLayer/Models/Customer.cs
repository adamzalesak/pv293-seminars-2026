namespace Kofi.DataAccessLayer.Models;

public class Customer
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Street { get; set; } = "";
    public string City { get; set; } = "";
    public string Zip { get; set; } = "";
    public DateTime RegisteredAt { get; set; }
    public bool IsVip { get; set; }

    // A token from the payment provider for the saved card, never the card
    // itself, and what the customer may see about it ("Visa ending 4242").
    public string? PaymentMethodToken { get; set; }
    public string? PaymentCardLabel { get; set; }

    public List<Order> Orders { get; set; } = new();
}
