namespace DotnetApiDddTemplate.Application.Features.Auth.Commands.Register;

/// <summary>
/// Handler for RegisterCommand.
/// Delegates to IAuthService for user creation and role assignment.
/// </summary>
public sealed class RegisterCommandHandler(
    IAuthService authService,
    ILogger<RegisterCommandHandler> logger) : IRequestHandler<RegisterCommand, Result<string>>
{
    public async Task<Result<string>> Handle(RegisterCommand request, CancellationToken ct)
    {
        logger.LogInformation("Registering user with email {Email}", request.Email);

        var result = await authService.RegisterAsync(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            ct);

        if (result.IsFailure)
            logger.LogWarning("Registration failed for {Email}: {Error}", request.Email, result.Error.Code);

        return result;
    }
}
