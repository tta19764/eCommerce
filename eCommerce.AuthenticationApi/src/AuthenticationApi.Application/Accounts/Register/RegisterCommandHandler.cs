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

namespace AuthenticationApi.Application.Accounts.Register;

/// <summary>
/// Handles account registration in Keycloak, the local auth store, and the user profile store.
/// </summary>
/// <param name="accountRepository">The repository that checks email uniqueness and tracks the account.</param>
/// <param name="roleRepository">The repository that resolves the local Customer role.</param>
/// <param name="unitOfWork">The unit of work that persists account state.</param>
/// <param name="identityProvider">The Keycloak boundary that creates and compensates the identity.</param>
/// <param name="userProfileClient">The UserApi client that creates the commerce profile.</param>
/// <param name="publishEndpoint">The bus endpoint that publishes confirmation-email work.</param>
/// <param name="cacheService">The cache used to invalidate administrator account pages.</param>
/// <param name="logger">The logger that records registration outcomes.</param>
/// <remarks>
/// Registration spans Keycloak, PostgreSQL, UserApi, cache storage, and the message broker without one distributed
/// transaction. The local account is committed before profile creation. Confirmation work is published only after
/// the profile link is committed. A missing local Customer role does not fail registration.
/// </remarks>
public sealed class RegisterCommandHandler(
    IAccountRepository accountRepository,
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork,
    IIdentityProvider identityProvider,
    IRequestClient<CreateUserProfileRequest> userProfileClient,
    IPublishEndpoint publishEndpoint,
    ICacheService cacheService,
    ILogger<RegisterCommandHandler> logger) : ICommandHandler<RegisterCommand, Guid>
{
    /// <summary>
    /// Registers a Customer identity, local account, and UserApi profile.
    /// </summary>
    /// <param name="request">The credentials and profile data. Email is normalized for local uniqueness checks.</param>
    /// <param name="cancellationToken">The token that cancels database, Keycloak, messaging, and cache operations.</param>
    /// <returns>The local account identifier, or a validation, duplicate, identity, or profile failure.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    /// <remarks>
    /// Profile-creation failure deletes the local account and Keycloak identity. Failure to link an already created
    /// profile performs the same compensation but does not delete that UserApi profile. Persistence or compensation
    /// failures can leave partial state. Publication failure can propagate after registration is fully committed.
    /// </remarks>
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
            await identityProvider.DeleteAsync(identityResult.Value, cancellationToken);
            return Result.Failure<Guid>(identityLinkResult.Error);
        }

        var customerRole = await roleRepository.GetByNameAsync(ApplicationRoles.Customer, cancellationToken);

        if (customerRole is not null)
        {
            accountResult.Value.AssignRole(customerRole);
        }

        accountRepository.Add(accountResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // UserApi owns profile data; AuthenticationApi is the only service that starts profile creation.
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
                "Profile creation failed for account {AccountId}: {ErrorCode}",
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

        logger.LogInformation("Registered account {AccountId}", accountId);

        return Result.Success(accountId);
    }
}
