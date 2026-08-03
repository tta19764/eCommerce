using AuthenticationApi.Domain.Accounts;
using AuthenticationApi.Messages.Accounts;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace AuthenticationApi.Application.Accounts.Messaging;

/// <summary>
/// Responds to service-to-service requests for account contact and confirmation state.
/// </summary>
public sealed class GetAccountContactByUserIdConsumer(
    IAccountRepository accountRepository,
    ILogger<GetAccountContactByUserIdConsumer> logger)
    : IConsumer<GetAccountContactByUserIdRequest>
{
    /// <summary>
    /// Handles an account contact lookup by linked user profile identifier.
    /// </summary>
    /// <param name="context">The MassTransit consume context.</param>
    public async Task Consume(ConsumeContext<GetAccountContactByUserIdRequest> context)
    {
        var account = await accountRepository.GetByUserIdAsync(
            context.Message.UserId,
            context.CancellationToken);

        if (account is null)
        {
            logger.LogWarning("Account linked to user {UserId} was not found", context.Message.UserId);

            await context.RespondAsync(new GetAccountContactByUserIdResponse(
                context.Message.UserId,
                null,
                string.Empty,
                false,
                false));

            return;
        }

        await context.RespondAsync(new GetAccountContactByUserIdResponse(
            context.Message.UserId,
            account.Id,
            account.Email.Value,
            account.IsEmailConfirmed,
            true));
    }
}
