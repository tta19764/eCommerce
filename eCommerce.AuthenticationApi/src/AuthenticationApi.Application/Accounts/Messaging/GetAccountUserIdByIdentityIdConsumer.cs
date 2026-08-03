using AuthenticationApi.Domain.Accounts;
using AuthenticationApi.Messages.Accounts;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace AuthenticationApi.Application.Accounts.Messaging;

/// <summary>
/// Defines the GetAccountUserIdByIdentityIdConsumer class used by this slice.
/// </summary>
public sealed class GetAccountUserIdByIdentityIdConsumer(
    IAccountRepository accountRepository,
    ILogger<GetAccountUserIdByIdentityIdConsumer> logger)
    : IConsumer<GetAccountUserIdByIdentityIdRequest>
{
    /// <summary>
    /// Executes the Consume operation.
    /// </summary>
    /// <param name="context">The context value.</param>
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
