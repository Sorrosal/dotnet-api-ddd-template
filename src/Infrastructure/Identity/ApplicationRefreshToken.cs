namespace DotnetApiDddTemplate.Infrastructure.Identity;

/// <summary>
/// Refresh token entity for JWT refresh token rotation.
/// Stores refresh tokens with expiry and revocation state.
/// </summary>
public sealed class ApplicationRefreshToken
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User ID owning this refresh token.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// The refresh token value (base64).
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Expiry time in UTC.
    /// </summary>
    public DateTime ExpiresAtUtc { get; set; }

    /// <summary>
    /// Is token revoked (logged out).
    /// </summary>
    public bool IsRevoked { get; set; }

    /// <summary>
    /// Creation time in UTC.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }
}
