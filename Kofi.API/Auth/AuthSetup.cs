using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

namespace Kofi.API.Auth;

public static class AuthSetup
{
    public static void AddKofiAuth(this WebApplicationBuilder builder)
    {
        var config = builder.Configuration;

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Keep the claim names as they are in the token ("email",
                // "role"), not the long XML-namespace names .NET maps them to.
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = config["Auth:Issuer"],
                    ValidAudience = config["Auth:Audience"],
                    IssuerSigningKey = SigningKey(config),
                    NameClaimType = "email",
                    RoleClaimType = "role",
                };
            });

        builder.Services.AddAuthorizationBuilder()
            .AddPolicy(Roles.Customer, policy => policy.RequireRole(Roles.Customer))
            .AddPolicy(Roles.Administrator, policy => policy.RequireRole(Roles.Administrator));
    }

    // Lets the Scalar reference send the token: log in, paste it once.
    public static OpenApiOptions AddBearerScheme(this OpenApiOptions options) =>
        options.AddDocumentTransformer((document, _, _) =>
        {
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
            };
            document.Security ??= new List<OpenApiSecurityRequirement>();
            document.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>(),
            });
            return Task.CompletedTask;
        });

    public static SymmetricSecurityKey SigningKey(IConfiguration config) =>
        new(Encoding.UTF8.GetBytes(config["Auth:SigningKey"]!));
}
