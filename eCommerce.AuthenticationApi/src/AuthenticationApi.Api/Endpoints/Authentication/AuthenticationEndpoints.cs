using AuthenticationApi.Application;
using AuthenticationApi.Application.Accounts;
using AuthenticationApi.Application.Accounts.ConfirmEmail;
using AuthenticationApi.Application.Accounts.DeleteAccount;
using AuthenticationApi.Application.Accounts.GetAccounts;
using AuthenticationApi.Application.Accounts.GetRoles;
using AuthenticationApi.Application.Accounts.Login;
using AuthenticationApi.Application.Accounts.RefreshToken;
using AuthenticationApi.Application.Accounts.Register;
using AuthenticationApi.Application.Accounts.RegisterAdmin;
using AuthenticationApi.Application.Accounts.RegisterSeller;
using AuthenticationApi.Domain.Accounts;
using MediatR;
using SharedLibrary.Api.Contracts;
using SharedLibrary.Api.Extensions;
using SharedLibrary.Application.Authorization;
using SharedLibrary.Application.Pagination;

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

        group.MapPost("register/admin", RegisterAdmin)
            .WithName(nameof(RegisterAdmin))
            .WithSummary("Register an administrator account")
            .Produces<ApiResponse<Guid>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<Guid>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .RequireAuthorization(ApplicationPermissions.AccountCreateAdmin);

        group.MapPost("register/seller", RegisterSeller)
            .WithName(nameof(RegisterSeller))
            .WithSummary("Register a seller account")
            .Produces<ApiResponse<Guid>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<Guid>>(StatusCodes.Status400BadRequest);

        group.MapPost("login", Login)
            .WithName(nameof(Login))
            .WithSummary("Log in with email and password")
            .Produces<ApiResponse<TokenResponse>>()
            .Produces<ApiResponse<TokenResponse>>(StatusCodes.Status401Unauthorized);

        group.MapPost("refresh", Refresh)
            .WithName(nameof(Refresh))
            .WithSummary("Refresh access and refresh tokens")
            .Produces<ApiResponse<TokenResponse>>()
            .Produces<ApiResponse<TokenResponse>>(StatusCodes.Status401Unauthorized);

        group.MapGet("confirm-email", ConfirmEmail)
            .WithName(nameof(ConfirmEmail))
            .WithSummary("Confirm an account email address")
            .Produces<ApiResponse<object>>()
            .Produces<ApiResponse<object>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<object>>(StatusCodes.Status409Conflict);

        group.MapGet("roles", GetRoles)
            .WithName(nameof(GetRoles))
            .WithSummary("Get a page of roles with permissions")
            .Produces<ApiResponse<PagedListResponse<RoleResponse>>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization(ApplicationPermissions.UserRead);

        group.MapGet("accounts", GetAccounts)
            .WithName(nameof(GetAccounts))
            .WithSummary("Get a page of accounts with linked user profile data")
            .Produces<ApiResponse<PagedListResponse<AccountResponse>>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization(ApplicationPermissions.UserRead);

        group.MapDelete("accounts/{accountId:guid}", DeleteAccount)
            .WithName(nameof(DeleteAccount))
            .WithSummary("Delete an account")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<object>>(StatusCodes.Status409Conflict)
            .RequireAuthorization(ApplicationPermissions.UserUpdate);

        return builder;
    }

    /// <summary>
    /// Executes the Register operation.
    /// </summary>
    /// <param name="command">The command value.</param>
    /// <param name="sender">The sender value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
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

    /// <summary>
    /// Executes the RegisterAdmin operation.
    /// </summary>
    /// <param name="command">The command value.</param>
    /// <param name="sender">The sender value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
    public static async Task<IResult> RegisterAdmin(
        RegisterAdminCommand command,
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

    /// <summary>
    /// Registers a public seller account.
    /// </summary>
    /// <param name="command">The seller registration command.</param>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    public static async Task<IResult> RegisterSeller(
        RegisterSellerCommand command,
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

    /// <summary>
    /// Executes the Login operation.
    /// </summary>
    /// <param name="command">The command value.</param>
    /// <param name="sender">The sender value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
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

    /// <summary>
    /// Executes the Refresh operation.
    /// </summary>
    /// <param name="command">The command value.</param>
    /// <param name="sender">The sender value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
    public static async Task<IResult> Refresh(
        RefreshTokenCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.MapToApiResponse())
            : Results.Unauthorized();
    }

    /// <summary>
    /// Executes the ConfirmEmail operation.
    /// </summary>
    /// <param name="accountId">The accountId value.</param>
    /// <param name="email">The email value.</param>
    /// <param name="sender">The sender value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
    public static async Task<IResult> ConfirmEmail(
        Guid accountId,
        string email,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ConfirmEmailCommand(accountId, email), cancellationToken);

        if (result.IsSuccess)
        {
            return Results.Ok(result.MapToApiResponse());
        }

        if (result.Error == AccountErrors.NotFound)
        {
            return Results.NotFound(result.MapToApiResponse());
        }

        return result.Error == AccountErrors.NotActive
            ? Results.Conflict(result.MapToApiResponse())
            : Results.BadRequest(result.MapToApiResponse());
    }

    /// <summary>
    /// Executes the GetRoles operation.
    /// </summary>
    /// <param name="sender">The sender value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
    /// <param name="page">The page value.</param>
    /// <param name="pageSize">The pageSize value.</param>
    public static async Task<IResult> GetRoles(
        ISender sender,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 10)
    {
        var result = await sender.Send(new GetRolesPageQuery(page, pageSize), cancellationToken);

        return Results.Ok(result.MapToApiResponse());
    }

    /// <summary>
    /// Executes the GetAccounts operation.
    /// </summary>
    /// <param name="sender">The sender value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
    /// <param name="page">The page value.</param>
    /// <param name="pageSize">The pageSize value.</param>
    public static async Task<IResult> GetAccounts(
        ISender sender,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 10)
    {
        var result = await sender.Send(new GetAccountsPageQuery(page, pageSize), cancellationToken);

        return Results.Ok(result.MapToApiResponse());
    }

    /// <summary>
    /// Executes the DeleteAccount operation.
    /// </summary>
    /// <param name="accountId">The accountId value.</param>
    /// <param name="sender">The sender value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
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
