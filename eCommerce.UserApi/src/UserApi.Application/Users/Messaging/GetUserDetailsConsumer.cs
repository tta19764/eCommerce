using MassTransit;
using Microsoft.Extensions.Logging;
using UserApi.Domain.Users;
using UserApi.Messages.Users;

namespace UserApi.Application.Users.Messaging;

/// <summary>
/// Responds to service-to-service requests for user profile details.
/// </summary>
public sealed class GetUserDetailsConsumer(
    IUserRepository userRepository,
    ILogger<GetUserDetailsConsumer> logger) : IConsumer<GetUserDetailsRequest>
{
    /// <summary>
    /// Handles a user-details request and returns profile data when the user exists.
    /// </summary>
    /// <param name="context">The MassTransit consume context.</param>
    public async Task Consume(ConsumeContext<GetUserDetailsRequest> context)
    {
        var user = await userRepository.GetByIdAsync(context.Message.UserId, context.CancellationToken);

        if (user is null)
        {
            logger.LogWarning("User {UserId} was not found for details request", context.Message.UserId);

            await context.RespondAsync(new GetUserDetailsResponse(
                context.Message.UserId,
                string.Empty,
                string.Empty,
                false));

            return;
        }

        await context.RespondAsync(UserMapper.ToDetailsResponse(user));
    }
}
