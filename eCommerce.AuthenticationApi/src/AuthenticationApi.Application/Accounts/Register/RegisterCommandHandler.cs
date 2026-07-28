using AuthenticationApi.Application.Abstractions;
using AuthenticationApi.Domain.Accounts;
using Microsoft.Extensions.Logging;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace AuthenticationApi.Application.Accounts.Register;

/// <summary>
/// Handles account registration in Keycloak and the local auth store.
/// </summary>
public sealed class RegisterCommandHandler(
    IAccountRepository accountRepository,
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork,
    IIdentityProvider identityProvider,
    ILogger<RegisterCommandHandler> logger) : ICommandHandler<RegisterCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        var existingAccount = await accountRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (existingAccount is not null)
        {
            return Result.Failure<Guid>(AccountErrors.DuplicateEmail);
        }

        var accountId = Guid.NewGuid();
        var accountResult = Account.Create(
            accountId,
            new FirstName(request.FirstName),
            new LastName(request.LastName),
            new Email(normalizedEmail));

        if (accountResult.IsFailure)
        {
            return Result.Failure<Guid>(accountResult.Error);
        }

        // Keycloak owns credentials and returns the identity subject that resource services can trust in tokens.
        var identityResult = await identityProvider.RegisterAsync(
            accountId,
            request.Email.Trim(),
            request.Password,
            request.FirstName,
            request.LastName,
            cancellationToken);

        if (identityResult.IsFailure)
        {
            logger.LogWarning(
                "Identity registration failed for account {AccountId}: {ErrorCode}",
                accountId,
                identityResult.Error.Code);

            return Result.Failure<Guid>(identityResult.Error);
        }

        var identityLinkResult = accountResult.Value.SetIdentityId(identityResult.Value);

        if (identityLinkResult.IsFailure)
        {
            await identityProvider.DeleteAsync(accountId, cancellationToken);
            return Result.Failure<Guid>(identityLinkResult.Error);
        }

        var customerRole = await roleRepository.GetByNameAsync("Customer", cancellationToken);

        if (customerRole is not null)
        {
            accountResult.Value.AssignRole(customerRole);
        }

        accountRepository.Add(accountResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Registered account {AccountId}", accountId);

        return Result.Success(accountId);
    }
}
