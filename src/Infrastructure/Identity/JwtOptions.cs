namespace DotnetApiDddTemplate.Infrastructure.Identity;

/// <summary>
/// JWT configuration options from appsettings.json [Jwt] section.
/// </summary>
public sealed class JwtOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// Secret key for signing JWT tokens (HMAC-SHA256).
    /// Must be at least 256 bits for HS256.
    /// </summary>
    public string Secret { get; init; } = string.Empty;

    /// <summary>
    /// JWT issuer claim.
    /// </summary>
    public string Issuer { get; init; } = string.Empty;

    /// <summary>
    /// JWT audience claim.
    /// </summary>
    public string Audience { get; init; } = string.Empty;

    /// <summary>
    /// Access token expiry in minutes (default 15).
    /// </summary>
    public int ExpiryMinutes { get; init; } = 15;

    /// <summary>
    /// Refresh token expiry in days (default 7).
    /// </summary>
    public int RefreshTokenExpiryDays { get; init; } = 7;
}
