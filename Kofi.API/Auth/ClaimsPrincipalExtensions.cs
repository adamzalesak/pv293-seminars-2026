using System.Security.Claims;

namespace Kofi.API.Auth;

public static class ClaimsPrincipalExtensions
{
    public static Guid? UserId(this ClaimsPrincipal user) =>
        Guid.TryParse(user.FindFirstValue("sub"), out var id) ? id : null;

    // The customer always comes from the token, never from the request.
    // Null for the administrator, who is not a customer.
    public static Guid? CustomerId(this ClaimsPrincipal user) =>
        Guid.TryParse(user.FindFirstValue("customer_id"), out var id) ? id : null;
}
