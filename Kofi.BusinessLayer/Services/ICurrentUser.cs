namespace Kofi.BusinessLayer.Services;

// Who is making the current request. The API knows it from the token.
public interface ICurrentUser
{
    // Null when the request carries no token, like the return from the
    // payment page.
    string? Email { get; }
}
