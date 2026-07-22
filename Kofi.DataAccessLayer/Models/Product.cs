namespace Kofi.DataAccessLayer.Models;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;

    // Path of the image served from wwwroot; null when there is none.
    public string? ImageUrl { get; set; }

    // Null for equipment and tea.
    public DateOnly? RoastedOn { get; set; }
    // Null for equipment and blends.
    public string? Origin { get; set; }
}
