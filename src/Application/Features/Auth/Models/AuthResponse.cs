namespace DotnetApiDddTemplate.Application.Features.Auth.Models;

/// <summary>
/// Response for successful authentication containing JWT and refresh token.
/// </summary>
public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    string UserId,
    string Email);
