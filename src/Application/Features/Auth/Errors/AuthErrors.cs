namespace DotnetApiDddTemplate.Application.Features.Auth.Errors;

/// <summary>
/// Authentication and authorization domain errors.
/// </summary>
public static class AuthErrors
{
    /// <summary>
    /// Invalid email or password during login.
    /// </summary>
    public static readonly Error InvalidCredentials =
        new("Auth.InvalidCredentials", "Invalid email or password");

    /// <summary>
    /// Email already exists during registration.
    /// </summary>
    public static readonly Error EmailAlreadyExists =
        new("Auth.EmailAlreadyExists", "An account with this email already exists");

    /// <summary>
    /// Refresh token is invalid, expired, or revoked.
    /// </summary>
    public static readonly Error InvalidRefreshToken =
        new("Auth.InvalidRefreshToken", "Refresh token is invalid or expired");

    /// <summary>
    /// User registration failed (validation or storage error).
    /// </summary>
    public static readonly Error RegistrationFailed =
        new("Auth.RegistrationFailed", "User registration failed");
}
