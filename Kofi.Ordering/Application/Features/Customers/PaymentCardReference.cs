namespace Kofi.Ordering.Application.Features.Customers;

// How a card setup at the provider names the customer whose card it replaces.
// A setup for a new subscription names the subscription instead.
public static class PaymentCardReference
{
    private const string Prefix = "customer/";

    public static string For(Guid customerId) => Prefix + customerId;

    public static Guid? CustomerOf(string reference) =>
        reference.StartsWith(Prefix) && Guid.TryParse(reference[Prefix.Length..], out var id) ? id : null;
}
