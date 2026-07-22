namespace Kofi.API.Auth;

// The roles a token can carry. Each one is also the name of the
// authorization policy that requires it.
public static class Roles
{
    public const string Customer = "Customer";
    public const string Administrator = "Administrator";
}
