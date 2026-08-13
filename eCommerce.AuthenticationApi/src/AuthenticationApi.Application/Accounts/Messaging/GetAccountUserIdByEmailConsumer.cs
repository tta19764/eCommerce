using AuthenticationApi.Domain.Accounts;
using AuthenticationApi.Messages.Accounts;
using MassTransit;

namespace AuthenticationApi.Application.Accounts.Messaging;

/// <summary>Resolves an account email address to its linked UserApi identifier.</summary>
public sealed class GetAccountUserIdByEmailConsumer(IAccountRepository repository)
    : IConsumer<GetAccountUserIdByEmailRequest>
{
    /// <inheritdoc />
    public async Task Consume(ConsumeContext<GetAccountUserIdByEmailRequest> context)
    {
        var email = context.Message.Email.Trim().ToUpperInvariant();
        var account = await repository.GetByEmailAsync(email, context.CancellationToken);
        await context.RespondAsync(new GetAccountUserIdByEmailResponse(
            account?.UserId is not null,
            account?.UserId));
    }
}
