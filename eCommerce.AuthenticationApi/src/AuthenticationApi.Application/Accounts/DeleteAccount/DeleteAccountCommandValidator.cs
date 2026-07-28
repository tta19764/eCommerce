using FluentValidation;

namespace AuthenticationApi.Application.Accounts.DeleteAccount;

/// <summary>
/// Validates account deletion requests.
/// </summary>
public sealed class DeleteAccountCommandValidator : AbstractValidator<DeleteAccountCommand>
{
    public DeleteAccountCommandValidator()
    {
        RuleFor(command => command.AccountId).NotEmpty();
    }
}

