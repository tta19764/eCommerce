using UserApi.Domain.Users;
using UserApi.Messages.Users;

namespace UserApi.Application.Users;

/// <summary>
/// Maps user aggregates to API and message read models.
/// </summary>
internal static class UserMapper
{
    /// <summary>
    /// Converts a user aggregate to an API response.
    /// </summary>
    /// <param name="user">The user aggregate.</param>
    /// <returns>The user response.</returns>
    internal static UserResponse ToResponse(User user)
    {
        return new UserResponse(
            user.Id,
            user.FirstName.Value,
            user.LastName.Value,
            user.FullName,
            user.Email.Value);
    }

    /// <summary>
    /// Converts a user aggregate to a message response.
    /// </summary>
    /// <param name="user">The user aggregate.</param>
    /// <returns>The user details message response.</returns>
    internal static GetUserDetailsResponse ToDetailsResponse(User user)
    {
        return new GetUserDetailsResponse(
            user.Id,
            user.FullName,
            user.Email.Value,
            true);
    }
}
