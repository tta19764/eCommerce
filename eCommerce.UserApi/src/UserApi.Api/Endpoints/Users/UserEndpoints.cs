using MediatR;
using SharedLibrary.Api.Contracts;
using SharedLibrary.Api.Extensions;
using UserApi.Api.Endpoints;
using UserApi.Application.Users;
using UserApi.Application.Users.GetUser;
using UserApi.Application.Users.UpdateUser;

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

        group.MapGet("{userId:guid}", GetUser)
            .WithName(nameof(GetUser))
            .Produces<ApiResponse<UserResponse>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<UserResponse>>(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        group.MapPut("{userId:guid}", UpdateUser)
            .WithName(nameof(UpdateUser))
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        return builder;
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

    /// <summary>
    /// Updates a user profile.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="request">The update-user request body.</param>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>An HTTP result indicating the update outcome.</returns>
    public static async Task<IResult> UpdateUser(
        Guid userId,
        UpdateUserRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateUserCommand(userId, request.FirstName, request.LastName, request.ImageId),
            cancellationToken);

        if (result.IsSuccess)
        {
            return Results.NoContent();
        }

        return result.Error.Code.EndsWith(".NotFound", StringComparison.Ordinal)
            ? Results.NotFound(result.MapToApiResponse())
            : Results.BadRequest(result.MapToApiResponse());
    }
}
