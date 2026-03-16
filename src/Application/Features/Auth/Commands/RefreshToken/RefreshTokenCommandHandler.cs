namespace DotnetApiDddTemplate.Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// Handler for RefreshTokenCommand.
/// Delegates to IAuthService for token refresh.
/// </summary>
public sealed class RefreshTokenCommandHandler(
    IAuthService authService,
    ILogger<RefreshTokenCommandHandler> logger) : IRequestHandler<RefreshTokenCommand, Result<AuthResponse>>
{
    public async Task<Result<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        logger.LogInformation("Refreshing token");
        return await authService.RefreshTokenAsync(request.RefreshToken, ct);
    }
}
