using FluentValidation;

namespace UserApi.Application.Users.UpdateUser;

/// <summary>
/// Validates update-user commands.
/// </summary>
public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    /// <summary>
    /// Creates validation rules for profile updates.
    /// </summary>
    public UpdateUserCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(command => command.LastName).NotEmpty().MaximumLength(100);
        RuleFor(command => command.ImageId).MaximumLength(100);
    }
}
