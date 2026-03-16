namespace DotnetApiDddTemplate.Application.Features.Auth.Interfaces;

/// <summary>
/// Authentication service abstraction.
/// Handles user registration, login, token refresh, and logout.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Register a new user.
    /// </summary>
    Task<Result<string>> RegisterAsync(
        string email,
        string password,
        string? firstName,
        string? lastName,
        CancellationToken ct = default);

    /// <summary>
    /// Login user and issue JWT and refresh tokens.
    /// </summary>
    Task<Result<AuthResponse>> LoginAsync(
        string email,
        string password,
        CancellationToken ct = default);

    /// <summary>
    /// Refresh JWT using refresh token.
    /// </summary>
    Task<Result<AuthResponse>> RefreshTokenAsync(
        string refreshToken,
        CancellationToken ct = default);

    /// <summary>
    /// Logout user by revoking refresh token.
    /// </summary>
    Task<Result> LogoutAsync(
        string refreshToken,
        CancellationToken ct = default);
}
