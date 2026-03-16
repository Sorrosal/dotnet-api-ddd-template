namespace DotnetApiDddTemplate.Infrastructure.Identity;

/// <summary>
/// Application user extending ASP.NET Identity IdentityUser.
/// Infrastructure-layer class for authentication and authorization.
/// </summary>
public sealed class ApplicationUser : IdentityUser
{
    /// <summary>
    /// User's first name.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// User's last name.
    /// </summary>
    public string? LastName { get; set; }
}
