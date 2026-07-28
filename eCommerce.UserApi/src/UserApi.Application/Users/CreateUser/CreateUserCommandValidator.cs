using FluentValidation;

namespace UserApi.Application.Users.CreateUser;

/// <summary>
/// Validates create-user commands.
/// </summary>
public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    /// <summary>
    /// Creates validation rules for required profile fields.
    /// </summary>
    public CreateUserCommandValidator()
    {
        RuleFor(command => command.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(command => command.LastName).NotEmpty().MaximumLength(100);
        RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(320);
    }
}
