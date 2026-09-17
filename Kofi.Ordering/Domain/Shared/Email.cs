namespace Kofi.Ordering.Domain.Shared;

// An address we can send a message to. The constructor refuses what cannot be
// one and stores it in a single spelling, so two customers cannot register
// the same address in different case. It does not prove that the mailbox
// exists: only a delivered message does.
public record Email
{
    // RFC 5321 limits a forward path to 256 octets, the address itself to 254.
    public const int MaxLength = 254;

    public Email(string value)
    {
        var address = (value ?? "").Trim().ToLowerInvariant();

        if (address.Length is 0 or > MaxLength)
        {
            throw new DomainRuleException("An e-mail address has between one and 254 characters.");
        }

        var at = address.IndexOf('@');
        if (at <= 0 || at != address.LastIndexOf('@') || at == address.Length - 1)
        {
            throw new DomainRuleException("An e-mail address is a name, one @ and a domain.");
        }

        var domain = address[(at + 1)..];
        if (address.Any(char.IsWhiteSpace) || !domain.Contains('.') || domain.StartsWith('.') || domain.EndsWith('.'))
        {
            throw new DomainRuleException("An e-mail address needs a domain such as example.com.");
        }

        Value = address;
    }

    public string Value { get; }

    public override string ToString() => Value;
}
