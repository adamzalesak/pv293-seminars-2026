using Kofi.Domain.Shared;

namespace Kofi.Domain.Customers;

public class Customer : AggregateRoot
{
    private Customer()
    {
    }

    public string Name { get; private set; } = "";
    public Email Email { get; private set; } = null!;
    public string Street { get; private set; } = "";
    public string City { get; private set; } = "";
    public string Zip { get; private set; } = "";
    public DateTime RegisteredAt { get; private set; }
    public bool IsVip { get; private set; }
    public string? PaymentMethodToken { get; private set; }
    public string? PaymentCardLabel { get; private set; }

    public static Customer Register(Guid id, string name, Email email, string street, string city, string zip,
        DateTime now, bool isVip = false, string? paymentMethodToken = null, string? paymentCardLabel = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainRuleException("A customer needs a name.");
        }

        return new Customer
        {
            Id = id,
            Name = name,
            Email = email,
            Street = street,
            City = city,
            Zip = zip,
            RegisteredAt = now,
            IsVip = isVip,
            PaymentMethodToken = paymentMethodToken,
            PaymentCardLabel = paymentCardLabel
        };
    }

    public void MakeVip() => IsVip = true;

    public void SavePaymentCard(string token, string? label)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new DomainRuleException("A saved card needs a provider token.");
        }

        PaymentMethodToken = token;
        PaymentCardLabel = label;
    }
}
