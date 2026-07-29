using AuthenticationApi.Application;
using AuthenticationApi.Application.Accounts;
using AuthenticationApi.Application.Accounts.DeleteAccount;
using AuthenticationApi.Application.Accounts.GetAccounts;
using AuthenticationApi.Application.Accounts.GetRoles;
using AuthenticationApi.Application.Accounts.Login;
using AuthenticationApi.Application.Accounts.Register;
using AuthenticationApi.Application.Common;
using AuthenticationApi.Domain.Accounts;
using MediatR;
using SharedLibrary.Api.Contracts;
using SharedLibrary.Api.Extensions;

namespace AuthenticationApi.Api.Endpoints.Authentication;

/// <summary>
/// Minimal API endpoints for authentication and account lifecycle operations.
/// </summary>
public static class AuthenticationEndpoints
{
    /// <summary>
    /// Maps authentication endpoints.
    /// </summary>
    /// <param name="builder">The endpoint route builder.</param>
    /// <returns>The endpoint route builder with authentication endpoints registered.</returns>
    public static IEndpointRouteBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("auth")
            .WithTags("Authentication")
            .HasApiVersion(AuthenticationApiApiVersions.V1);

        group.MapPost("register", Register)
            .WithName(nameof(Register))
            .WithSummary("Register an account")
            .Produces<ApiResponse<Guid>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<Guid>>(StatusCodes.Status400BadRequest);

        group.MapPost("login", Login)
            .WithName(nameof(Login))
            .WithSummary("Log in with email and password")
            .Produces<ApiResponse<TokenResponse>>()
            .Produces<ApiResponse<TokenResponse>>(StatusCodes.Status401Unauthorized);

        group.MapGet("roles", GetRoles)
            .WithName(nameof(GetRoles))
            .WithSummary("Get a page of roles with permissions")
            .Produces<ApiResponse<PagedListResponse<RoleResponse>>>();

        group.MapGet("accounts", GetAccounts)
            .WithName(nameof(GetAccounts))
            .WithSummary("Get a page of accounts with linked user profile data")
            .Produces<ApiResponse<PagedListResponse<AccountResponse>>>();

        group.MapDelete("accounts/{accountId:guid}", DeleteAccount)
            .WithName(nameof(DeleteAccount))
            .WithSummary("Delete an account")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<object>>(StatusCodes.Status409Conflict);

        return builder;
    }

    public static async Task<IResult> Register(
        RegisterCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.CreatedAtRoute(
                nameof(DeleteAccount),
                new { accountId = result.Value, version = AuthenticationApiApiVersions.V1RouteValue },
                result.MapToApiResponse())
            : Results.BadRequest(result.MapToApiResponse());
    }

    public static async Task<IResult> Login(
        LoginCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.MapToApiResponse())
            : Results.Unauthorized();
    }

    public static async Task<IResult> GetRoles(
        ISender sender,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 10)
    {
        var result = await sender.Send(new GetRolesPageQuery(page, pageSize), cancellationToken);

        return Results.Ok(result.MapToApiResponse());
    }

    public static async Task<IResult> GetAccounts(
        ISender sender,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 10)
    {
        var result = await sender.Send(new GetAccountsPageQuery(page, pageSize), cancellationToken);

        return Results.Ok(result.MapToApiResponse());
    }

    public static async Task<IResult> DeleteAccount(
        Guid accountId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteAccountCommand(accountId), cancellationToken);

        if (result.IsSuccess)
        {
            return Results.NoContent();
        }

        return result.Error == AccountErrors.NotFound
            ? Results.NotFound(result.MapToApiResponse())
            : Results.Conflict(result.MapToApiResponse());
    }
}
