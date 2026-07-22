using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Kofi.API.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/auth/login", Results<Ok<LoginResponse>, UnauthorizedHttpResult> (LoginRequest request, IConfiguration config) =>
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return TypedResults.Unauthorized();
            }

            var accounts = config.GetSection("Auth:Accounts").Get<List<DemoAccount>>() ?? new();
            var account = accounts.SingleOrDefault(a =>
                string.Equals(a.Email, request.Email, StringComparison.OrdinalIgnoreCase));

            if (account == null || new PasswordHasher<DemoAccount>()
                    .VerifyHashedPassword(account, account.PasswordHash, request.Password)
                    == PasswordVerificationResult.Failed)
            {
                return TypedResults.Unauthorized();
            }

            var claims = new List<Claim>
            {
                new("sub", account.Id.ToString()),
                new("email", account.Email),
                new("role", account.Role),
            };
            if (account.CustomerId != null)
            {
                claims.Add(new("customer_id", account.CustomerId.Value.ToString()));
            }

            var expiresAt = DateTime.UtcNow.AddHours(8);
            var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
            {
                Issuer = config["Auth:Issuer"],
                Audience = config["Auth:Audience"],
                Subject = new ClaimsIdentity(claims),
                Expires = expiresAt,
                SigningCredentials = new SigningCredentials(
                    AuthSetup.SigningKey(config), SecurityAlgorithms.HmacSha256),
            });

            return TypedResults.Ok(new LoginResponse { AccessToken = token, ExpiresAt = expiresAt });
        });
    }
}
