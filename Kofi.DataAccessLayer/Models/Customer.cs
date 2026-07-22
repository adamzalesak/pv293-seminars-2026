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

    public List<Order> Orders { get; set; } = new();
}
