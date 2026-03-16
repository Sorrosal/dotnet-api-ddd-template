namespace DotnetApiDddTemplate.Application.Features.Auth.Commands.Login;

/// <summary>
/// Handler for LoginCommand.
/// Delegates to IAuthService for credential validation and token generation.
/// </summary>
public sealed class LoginCommandHandler(
    IAuthService authService,
    ILogger<LoginCommandHandler> logger) : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken ct)
    {
        logger.LogInformation("Login attempt for {Email}", request.Email);
        return await authService.LoginAsync(request.Email, request.Password, ct);
    }
}
