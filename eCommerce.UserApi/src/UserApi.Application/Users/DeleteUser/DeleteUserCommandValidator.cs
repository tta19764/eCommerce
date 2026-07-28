using FluentValidation;

namespace UserApi.Application.Users.DeleteUser;

/// <summary>
/// Validates delete-user commands.
/// </summary>
public sealed class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    /// <summary>
    /// Creates validation rules for user deletion.
    /// </summary>
    public DeleteUserCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
    }
}
