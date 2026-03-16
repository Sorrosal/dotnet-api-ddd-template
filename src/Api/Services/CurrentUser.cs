namespace DotnetApiDddTemplate.Api.Services;

/// <summary>
/// Implementation of ICurrentUser that extracts user from HttpContext.
/// </summary>
public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private readonly HttpContext? _httpContext = httpContextAccessor.HttpContext;

    public string? Id => _httpContext?.User.FindFirst("sub")?.Value;

    public string? Email => _httpContext?.User.FindFirst("email")?.Value;

    public string? Name => _httpContext?.User.FindFirst("name")?.Value;

    public bool IsAuthenticated => _httpContext?.User.Identity?.IsAuthenticated ?? false;
}
