using MediatR;
using SharedLibrary.Api.Contracts;
using SharedLibrary.Api.Extensions;
using UserApi.Api.Endpoints;
using UserApi.Application.Users;
using UserApi.Application.Users.CreateUser;
using UserApi.Application.Users.GetUser;

namespace UserApi.Api.Endpoints.Users;

/// <summary>
/// Minimal API endpoints for user profile management.
/// </summary>
public static class UserEndpoints
{
    /// <summary>
    /// Maps user profile endpoints.
    /// </summary>
    /// <param name="builder">The endpoint route builder.</param>
    /// <returns>The endpoint route builder with user endpoints registered.</returns>
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("users")
            .WithTags("Users")
            .HasApiVersion(UserApiApiVersions.V1);

        group.MapPost(string.Empty, CreateUser)
            .WithName(nameof(CreateUser))
            .Produces<ApiResponse<Guid>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<Guid>>(StatusCodes.Status400BadRequest);

        group.MapGet("{userId:guid}", GetUser)
            .WithName(nameof(GetUser))
            .Produces<ApiResponse<UserResponse>>()
            .Produces<ApiResponse<UserResponse>>(StatusCodes.Status404NotFound);

        return builder;
    }

    /// <summary>
    /// Creates a user profile.
    /// </summary>
    /// <param name="command">The create-user command.</param>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>An HTTP result containing the created user identifier or validation errors.</returns>
    public static async Task<IResult> CreateUser(
        CreateUserCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.CreatedAtRoute(
                nameof(GetUser),
                new { userId = result.Value, version = UserApiApiVersions.V1RouteValue },
                result.MapToApiResponse())
            : Results.BadRequest(result.MapToApiResponse());
    }

    /// <summary>
    /// Gets a user profile by identifier.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>An HTTP result containing user details or a not-found error.</returns>
    public static async Task<IResult> GetUser(
        Guid userId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetUserQuery(userId), cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.MapToApiResponse())
            : Results.NotFound(result.MapToApiResponse());
    }
}
