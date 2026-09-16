using Kofi.Application.Services;

namespace Kofi.Auth;

public class HttpCurrentUser(IHttpContextAccessor http) : ICurrentUser
{
    public Guid? UserId => http.HttpContext?.User.UserId();

    public Guid? CustomerId => http.HttpContext?.User.CustomerId();
}
