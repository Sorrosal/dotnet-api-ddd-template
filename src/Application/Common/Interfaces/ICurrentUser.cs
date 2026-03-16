namespace DotnetApiDddTemplate.Application.Common.Interfaces;

/// <summary>
/// Interface for accessing current user information.
/// Implemented by API middleware to extract user from HttpContext.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// Get current user ID.
    /// </summary>
    string? Id { get; }

    /// <summary>
    /// Get current user email.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Get current user name.
    /// </summary>
    string? Name { get; }

    /// <summary>
    /// Check if user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }
}
