namespace Kofi.Catalog.Domain.Products;

public class Product : AggregateRoot
{
    private Product()
    {
    }

    public string Name { get; private set; } = "";
    public string Description { get; private set; } = "";
    public Price Price { get; private set; } = null!;
    public ProductStatus Status { get; private set; }
    public string? ImageUrl { get; private set; }
    public DateOnly? RoastedOn { get; private set; }
    public string? Origin { get; private set; }

    public static Product Create(Guid id, string name, string description, Price price,
        ProductStatus status = ProductStatus.Draft, DateOnly? roastedOn = null, string? origin = null, string? imageUrl = null)
    {
        var product = new Product { Id = id, Price = price, Status = status };
        product.UpdateDetails(name, description, roastedOn, origin, imageUrl);

        return product;
    }

    public void UpdateDetails(string name, string description, DateOnly? roastedOn, string? origin, string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainRuleException("A product needs a name.");
        }

        Name = name;
        Description = description;
        RoastedOn = roastedOn;
        Origin = origin;
        ImageUrl = imageUrl;
    }

    public void ChangePrice(Price price) => Price = price;
    public void Activate() => Status = ProductStatus.Active;
    public void Deactivate() => Status = ProductStatus.Inactive;

    public void Discontinue()
    {
        if (Status != ProductStatus.Active)
        {
            throw new DomainRuleException("Only an active product can be discontinued.");
        }

        Status = ProductStatus.Discontinued;
    }
}
