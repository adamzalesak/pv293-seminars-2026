using System.Text.RegularExpressions;

namespace Kofi.Ordering.Domain.Shared;

public record ShippingAddress
{
    private ShippingAddress()
    {
    }

    public ShippingAddress(string street, string city, string zip)
    {
        street = (street ?? "").Trim();
        city = (city ?? "").Trim();
        zip = (zip ?? "").Trim();

        if (!Regex.IsMatch(street, @"\p{L}") || !Regex.IsMatch(street, @"[0-9]"))
        {
            throw new DomainRuleException("The street needs a name and a house number.");
        }

        if (city.Length == 0)
        {
            throw new DomainRuleException("The city is required.");
        }

        if (!Regex.IsMatch(zip, @"^(?:[0-9]{5}|[0-9]{3} [0-9]{2})$"))
        {
            throw new DomainRuleException("A postal code has five digits.");
        }

        Street = street;
        City = city;
        Zip = zip.Replace(" ", "");
    }

    public string Street { get; } = "";
    public string City { get; } = "";
    public string Zip { get; } = "";
}
