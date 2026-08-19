using AuthenticationApi.Application.Abstractions;
using AuthenticationApi.Domain.Accounts;
using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationApi.Messages.Emails;
using SharedLibrary.Application.Abstractions.Caching;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Application.Authorization;
using SharedLibrary.Domain.Abstractions;
using UserApi.Messages.Users;

namespace AuthenticationApi.Application.Accounts.RegisterSeller;

/// <summary>
/// Handles seller registration in Keycloak, the local auth store, and the user profile store.
/// </summary>
/// <param name="accountRepository">The repository that checks email uniqueness and tracks the account.</param>
/// <param name="roleRepository">The repository that resolves the local Seller role.</param>
/// <param name="unitOfWork">The unit of work that persists account state.</param>
/// <param name="identityProvider">The Keycloak boundary that creates and compensates the identity.</param>
/// <param name="userProfileClient">The UserApi client that creates the commerce profile.</param>
/// <param name="publishEndpoint">The bus endpoint that publishes confirmation-email work.</param>
/// <param name="cacheService">The cache used to invalidate administrator account pages.</param>
/// <param name="logger">The logger that records registration outcomes.</param>
/// <remarks>
/// This handler creates Seller credentials and a profile only; it does not create a SellerApi application or store.
/// The operation has the same non-transactional compensation boundaries as customer registration. A missing local
/// Seller role does not fail registration.
/// </remarks>
public sealed class RegisterSellerCommandHandler(
    IAccountRepository accountRepository,
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork,
    IIdentityProvider identityProvider,
    IRequestClient<CreateUserProfileRequest> userProfileClient,
    IPublishEndpoint publishEndpoint,
    ICacheService cacheService,
    ILogger<RegisterSellerCommandHandler> logger) : ICommandHandler<RegisterSellerCommand, Guid>
{
    /// <summary>Registers a Seller identity, local account, and UserApi profile.</summary>
    /// <param name="request">The credentials and profile data.</param>
    /// <param name="cancellationToken">The token that cancels database, Keycloak, messaging, and cache operations.</param>
    /// <returns>The local account identifier, or a validation, duplicate, identity, or profile failure.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    /// <remarks>
    /// A failed profile-link operation can leave an orphan UserApi profile. Publication failure can propagate after
    /// the identity, account, and profile link are committed.
    /// </remarks>
    public async Task<Result<Guid>> Handle(RegisterSellerCommand request, CancellationToken cancellationToken)
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
            new Email(normalizedEmail));

        if (accountResult.IsFailure)
        {
            return Result.Failure<Guid>(accountResult.Error);
        }

        var identityResult = await identityProvider.RegisterAsync(
            accountId,
            request.Email.Trim(),
            request.Password,
            request.FirstName,
            request.LastName,
            ApplicationRoles.Seller,
            cancellationToken);

        if (identityResult.IsFailure)
        {
            logger.LogWarning(
                "Seller identity registration failed for account {AccountId}: {ErrorCode}",
                accountId,
                identityResult.Error.Code);

            return Result.Failure<Guid>(identityResult.Error);
        }

        var identityLinkResult = accountResult.Value.SetIdentityId(identityResult.Value);

        if (identityLinkResult.IsFailure)
        {
            await identityProvider.DeleteAsync(identityResult.Value, cancellationToken);
            return Result.Failure<Guid>(identityLinkResult.Error);
        }

        var sellerRole = await roleRepository.GetByNameAsync(ApplicationRoles.Seller, cancellationToken);

        if (sellerRole is not null)
        {
            accountResult.Value.AssignRole(sellerRole);
        }

        accountRepository.Add(accountResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var profile = await userProfileClient.GetResponse<CreateUserProfileResponse>(
            new CreateUserProfileRequest(
                request.FirstName,
                request.LastName,
                request.Email.Trim()),
            cancellationToken);

        if (!profile.Message.Created)
        {
            accountRepository.Delete(accountResult.Value);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await AccountCacheKeys.InvalidatePagesAsync(cacheService, cancellationToken);
            await identityProvider.DeleteAsync(identityResult.Value, cancellationToken);

            logger.LogWarning(
                "Seller profile creation failed for account {AccountId}: {ErrorCode}",
                accountId,
                profile.Message.ErrorCode);

            return Result.Failure<Guid>(AccountErrors.ProfileCreationFailed);
        }

        var profileLinkResult = accountResult.Value.SetUserId(profile.Message.UserId);

        if (profileLinkResult.IsFailure)
        {
            accountRepository.Delete(accountResult.Value);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await AccountCacheKeys.InvalidatePagesAsync(cacheService, cancellationToken);
            await identityProvider.DeleteAsync(identityResult.Value, cancellationToken);

            return Result.Failure<Guid>(profileLinkResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await AccountCacheKeys.InvalidatePagesAsync(cacheService, cancellationToken);

        await publishEndpoint.Publish(
            new SendEmailConfirmationRequest(
                accountId,
                request.Email.Trim(),
                request.FirstName,
                request.LastName),
            cancellationToken);

        logger.LogInformation("Registered seller account {AccountId}", accountId);

        return Result.Success(accountId);
    }
}
