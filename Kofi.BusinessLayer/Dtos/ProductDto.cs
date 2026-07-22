namespace Kofi.BusinessLayer.Dtos;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
    public string? ImageUrl { get; set; }
    public DateOnly? RoastedOn { get; set; }
    public string? Origin { get; set; }
}
