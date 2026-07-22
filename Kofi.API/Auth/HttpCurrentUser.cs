using Kofi.BusinessLayer.Services;

namespace Kofi.API.Auth;

public class HttpCurrentUser(IHttpContextAccessor http) : ICurrentUser
{
    public string? Email => http.HttpContext?.User.Email();
}
