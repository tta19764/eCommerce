using Microsoft.Extensions.Logging;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;
using UserApi.Domain.Users;

namespace UserApi.Application.Users.CreateUser;

/// <summary>
/// Handles user profile creation.
/// </summary>
public sealed class CreateUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ILogger<CreateUserCommandHandler> logger) : ICommandHandler<CreateUserCommand, Guid>
{
    /// <summary>
    /// Creates and persists a user profile.
    /// </summary>
    /// <param name="request">The create-user command.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The created user identifier, or a failure result.</returns>
    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var userResult = User.Create(
            new FirstName(request.FirstName.Trim()),
            new LastName(request.LastName.Trim()),
            new Email(request.Email.Trim()));

        if (userResult.IsFailure)
        {
            return Result.Failure<Guid>(userResult.Error);
        }

        userRepository.Add(userResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created user {UserId}", userResult.Value.Id);

        return Result.Success(userResult.Value.Id);
    }
}
