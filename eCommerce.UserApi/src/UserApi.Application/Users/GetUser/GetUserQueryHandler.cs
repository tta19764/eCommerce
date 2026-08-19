using Microsoft.Extensions.Logging;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;
using UserApi.Domain.Users;

namespace UserApi.Application.Users.GetUser;

/// <summary>
/// Handles single-user profile queries.
/// </summary>
/// <param name="userRepository">The repository that reads user profiles.</param>
/// <param name="logger">The logger that records missing profiles.</param>
public sealed class GetUserQueryHandler(
    IUserRepository userRepository,
    ILogger<GetUserQueryHandler> logger) : IQueryHandler<GetUserQuery, UserResponse>
{
    /// <summary>
    /// Reads one user profile.
    /// </summary>
    /// <param name="request">The user query.</param>
    /// <param name="cancellationToken">The token that cancels the repository query.</param>
    /// <returns>The user profile, or a not-found failure.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    public async Task<Result<UserResponse>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            logger.LogWarning("User {UserId} was not found", request.UserId);
            return Result.Failure<UserResponse>(UserErrors.NotFound);
        }

        return Result.Success(UserMapper.ToResponse(user));
    }
}
