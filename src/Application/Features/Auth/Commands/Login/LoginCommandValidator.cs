namespace DotnetApiDddTemplate.Application.Features.Auth.Commands.Login;

/// <summary>
/// Validator for LoginCommand.
/// </summary>
public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty();
    }
}
