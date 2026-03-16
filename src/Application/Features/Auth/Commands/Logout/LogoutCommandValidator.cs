namespace DotnetApiDddTemplate.Application.Features.Auth.Commands.Logout;

/// <summary>
/// Validator for LogoutCommand.
/// </summary>
public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty();
    }
}
