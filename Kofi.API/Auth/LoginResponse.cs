namespace Kofi.API.Auth;

public record LoginResponse
{
    public required string AccessToken { get; init; }
    public required DateTime ExpiresAt { get; init; }
}
