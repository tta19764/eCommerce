using FluentValidation;

namespace AuthenticationApi.Application.Accounts.RefreshToken;

/// <summary>
/// Validates refresh-token requests.
/// </summary>
public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(command => command.RefreshToken).NotEmpty();
    }
}
