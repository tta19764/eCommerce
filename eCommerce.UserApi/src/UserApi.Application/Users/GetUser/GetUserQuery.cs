using SharedLibrary.Application.Abstractions.Messaging;

namespace UserApi.Application.Users.GetUser;

/// <summary>
/// Query for reading one user profile.
/// </summary>
/// <param name="UserId">The user identifier.</param>
public sealed record GetUserQuery(Guid UserId) : IQuery<UserResponse>;
