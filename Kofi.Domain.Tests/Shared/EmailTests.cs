using Kofi.Domain.Shared;
using Kofi.Domain;
using Xunit;

namespace Kofi.Domain.Tests.Shared;

// An address that cannot receive a message cannot be made.
public class EmailTests
{
    [Fact]
    public void An_address_is_stored_in_one_spelling()
    {
        Assert.Equal(new Email("petra@example.com"), new Email("  Petra@Example.COM "));
    }

    [Theory]
    [InlineData("")]
    [InlineData("petra")]
    [InlineData("@example.com")]
    [InlineData("petra@")]
    [InlineData("petra@@example.com")]
    [InlineData("pe tra@example.com")]
    [InlineData("petra@localhost")]
    [InlineData("petra@example.")]
    public void Something_that_cannot_receive_a_message_is_rejected(string value)
    {
        Assert.Throws<DomainRuleException>(() => new Email(value));
    }

    [Fact]
    public void An_address_fits_the_limit_of_the_mail_protocol()
    {
        var local = new string('a', 64);
        var domain = new string('b', Email.MaxLength - 64 - 1 - 4) + ".com";
        Assert.Equal(Email.MaxLength, new Email($"{local}@{domain}").Value.Length);
        Assert.Throws<DomainRuleException>(() => new Email($"{local}@b{domain}"));
    }
}
