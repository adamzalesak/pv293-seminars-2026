namespace Kofi.API.Auth;

// Demo accounts. They live in configuration, not in the database: there is
// no sign-up, so there is nothing to store.
public record DemoAccount
{
    public Guid Id { get; init; }
    public string Email { get; init; } = "";
    public string PasswordHash { get; init; } = "";
    public string Role { get; init; } = "";
    public Guid? CustomerId { get; init; }
}
