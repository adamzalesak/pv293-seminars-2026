namespace Kofi.DataAccessLayer.Models;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;

    // Served by the API from wwwroot; null shows the product's glyph instead.
    public string? ImageUrl { get; set; }

    // Coffee only; null for equipment and tea.
    public DateOnly? RoastedOn { get; set; }
    public string? Origin { get; set; }
}
