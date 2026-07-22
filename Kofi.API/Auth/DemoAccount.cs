namespace Kofi.API.Auth;

// The two accounts the course runs with. They live in appsettings.json, not
// in the database: there is no sign-up, so there is nothing to store.
public record DemoAccount
{
    public string Email { get; init; } = "";
    public string PasswordHash { get; init; } = "";
    public string Role { get; init; } = "";
    public Guid? CustomerId { get; init; }
}
