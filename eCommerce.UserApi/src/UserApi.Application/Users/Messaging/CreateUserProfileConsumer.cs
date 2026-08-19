using MassTransit;
using Microsoft.Extensions.Logging;
using SharedLibrary.Domain.Abstractions;
using UserApi.Domain.Users;
using UserApi.Messages.Users;

namespace UserApi.Application.Users.Messaging;

/// <summary>
/// Creates a profile for an identity that was created by AuthenticationApi.
/// </summary>
/// <param name="userRepository">The repository that tracks the new profile.</param>
/// <param name="unitOfWork">The unit of work that persists the profile.</param>
/// <param name="logger">The logger that records successful creation.</param>
/// <remarks>
/// Domain validation failures are returned as response data and are not thrown. The consumer does not correlate
/// the request to an existing account or profile, so request redelivery can create another profile.
/// </remarks>
public sealed class CreateUserProfileConsumer(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ILogger<CreateUserProfileConsumer> logger) : IConsumer<CreateUserProfileRequest>
{
    /// <summary>
    /// Validates and creates the requested user profile.
    /// </summary>
    /// <param name="context">The consume context that contains the required names and email address.</param>
    /// <returns>A task that completes after the profile is committed and the response is sent.</returns>
    /// <exception cref="OperationCanceledException">Message processing is canceled.</exception>
    public async Task Consume(ConsumeContext<CreateUserProfileRequest> context)
    {
        var userResult = User.Create(
            new FirstName(context.Message.FirstName.Trim()),
            new LastName(context.Message.LastName.Trim()),
            new Email(context.Message.Email.Trim()));

        if (userResult.IsFailure)
        {
            await context.RespondAsync(new CreateUserProfileResponse(
                Guid.Empty,
                false,
                userResult.Error.Code,
                userResult.Error.Name));

            return;
        }

        userRepository.Add(userResult.Value);
        await unitOfWork.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation(
            "Created user profile {UserId}",
            userResult.Value.Id);

        await context.RespondAsync(new CreateUserProfileResponse(
            userResult.Value.Id,
            true,
            null,
            null));
    }
}
