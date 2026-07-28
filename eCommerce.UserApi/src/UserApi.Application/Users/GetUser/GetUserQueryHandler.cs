using Microsoft.Extensions.Logging;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;
using UserApi.Domain.Users;

namespace UserApi.Application.Users.GetUser;

/// <summary>
/// Handles single-user profile queries.
/// </summary>
public sealed class GetUserQueryHandler(
    IUserRepository userRepository,
    ILogger<GetUserQueryHandler> logger) : IQueryHandler<GetUserQuery, UserResponse>
{
    /// <summary>
    /// Reads one user profile.
    /// </summary>
    /// <param name="request">The user query.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The user profile, or a not-found failure.</returns>
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
