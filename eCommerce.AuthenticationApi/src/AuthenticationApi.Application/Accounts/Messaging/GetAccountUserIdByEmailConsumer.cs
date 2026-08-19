using AuthenticationApi.Domain.Accounts;
using AuthenticationApi.Messages.Accounts;
using MassTransit;

namespace AuthenticationApi.Application.Accounts.Messaging;

/// <summary>Resolves an account email address to its linked UserApi identifier.</summary>
/// <param name="repository">The repository that resolves normalized account email.</param>
/// <remarks>The response is not-found when the account exists but has no linked profile.</remarks>
public sealed class GetAccountUserIdByEmailConsumer(IAccountRepository repository)
    : IConsumer<GetAccountUserIdByEmailRequest>
{
    /// <summary>Resolves a case-insensitive email to its linked UserApi identifier.</summary>
    /// <param name="context">The consume context that contains the account email.</param>
    /// <returns>A task that completes after the response is sent.</returns>
    /// <exception cref="OperationCanceledException">Message processing is canceled.</exception>
    public async Task Consume(ConsumeContext<GetAccountUserIdByEmailRequest> context)
    {
        var email = context.Message.Email.Trim().ToUpperInvariant();
        var account = await repository.GetByEmailAsync(email, context.CancellationToken);
        await context.RespondAsync(new GetAccountUserIdByEmailResponse(
            account?.UserId is not null,
            account?.UserId));
    }
}
