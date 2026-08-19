using Microsoft.Extensions.Logging;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;
using UserApi.Domain.Users;

namespace UserApi.Application.Users.CreateUser;

/// <summary>
/// Handles user profile creation.
/// </summary>
/// <param name="userRepository">The repository that tracks the new profile.</param>
/// <param name="unitOfWork">The unit of work that persists the profile.</param>
/// <param name="logger">The logger that records successful creation.</param>
/// <remarks>The handler does not perform an email or account-level duplicate check.</remarks>
public sealed class CreateUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ILogger<CreateUserCommandHandler> logger) : ICommandHandler<CreateUserCommand, Guid>
{
    /// <summary>
    /// Creates and persists a user profile.
    /// </summary>
    /// <param name="request">The create-user command.</param>
    /// <param name="cancellationToken">The token that cancels persistence.</param>
    /// <returns>The created user identifier, or a domain validation failure.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
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
