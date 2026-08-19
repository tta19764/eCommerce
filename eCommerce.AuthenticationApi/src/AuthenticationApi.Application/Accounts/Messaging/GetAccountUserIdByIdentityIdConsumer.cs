using AuthenticationApi.Domain.Accounts;
using AuthenticationApi.Messages.Accounts;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace AuthenticationApi.Application.Accounts.Messaging;

/// <summary>
/// Resolves a Keycloak subject to its linked UserApi profile identifier.
/// </summary>
/// <param name="accountRepository">The repository that resolves accounts by identity identifier.</param>
/// <param name="logger">The logger that records missing or incomplete links.</param>
public sealed class GetAccountUserIdByIdentityIdConsumer(
    IAccountRepository accountRepository,
    ILogger<GetAccountUserIdByIdentityIdConsumer> logger)
    : IConsumer<GetAccountUserIdByIdentityIdRequest>
{
    /// <summary>
    /// Resolves the identity and returns whether a complete profile link exists.
    /// </summary>
    /// <param name="context">The consume context that contains the Keycloak subject.</param>
    /// <returns>A task that completes after the response is sent.</returns>
    /// <exception cref="OperationCanceledException">Message processing is canceled.</exception>
    public async Task Consume(ConsumeContext<GetAccountUserIdByIdentityIdRequest> context)
    {
        if (string.IsNullOrWhiteSpace(context.Message.IdentityId))
        {
            await context.RespondAsync(new GetAccountUserIdByIdentityIdResponse(
                context.Message.IdentityId,
                null,
                false));

            return;
        }

        var account = await accountRepository.GetByIdentityIdAsync(
            context.Message.IdentityId,
            context.CancellationToken);

        if (account?.UserId is null)
        {
            logger.LogWarning(
                "Account with identity id {IdentityId} was not found or has no linked user profile",
                context.Message.IdentityId);

            await context.RespondAsync(new GetAccountUserIdByIdentityIdResponse(
                context.Message.IdentityId,
                null,
                false));

            return;
        }

        await context.RespondAsync(new GetAccountUserIdByIdentityIdResponse(
            account.IdentityId,
            account.UserId.Value,
            true));
    }
}
