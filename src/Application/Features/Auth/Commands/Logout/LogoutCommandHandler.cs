namespace DotnetApiDddTemplate.Application.Features.Auth.Commands.Logout;

/// <summary>
/// Handler for LogoutCommand.
/// Delegates to IAuthService to revoke the refresh token.
/// </summary>
public sealed class LogoutCommandHandler(
    IAuthService authService,
    ILogger<LogoutCommandHandler> logger) : IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken ct)
    {
        logger.LogInformation("Logging out user");
        return await authService.LogoutAsync(request.RefreshToken, ct);
    }
}
