using System.Security.Claims;
using AuthenticationApi.Messages.Accounts;
using MassTransit;
using MediatR;
using MessagingApi.Application.Conversations;
using MessagingApi.Application.Conversations.GetConversationMessagesPage;
using MessagingApi.Application.Conversations.GetConversationsPage;
using MessagingApi.Application.Conversations.MarkConversationRead;
using MessagingApi.Application.Conversations.SendConversationMessage;
using MessagingApi.Application.Conversations.StartProductInquiry;
using MessagingApi.Application.Conversations.StartSellerOrderConversation;
using MessagingApi.Domain.Conversations;
using SharedLibrary.Api.Contracts;
using SharedLibrary.Api.Extensions;
using SharedLibrary.Application.Pagination;

namespace MessagingApi.Api.Endpoints.Conversations;

/// <summary>
/// Minimal API endpoints for customer-seller marketplace conversations.
/// </summary>
public static class ConversationEndpoints
{
    /// <summary>
    /// Maps conversation endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapConversationEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("conversations")
            .WithTags("Conversations")
            .HasApiVersion(MessagingApiApiVersions.V1)
            .RequireAuthorization();

        group.MapPost("product-inquiries/{productId:guid}", StartProductInquiry)
            .WithName(nameof(StartProductInquiry))
            .WithSummary("Start or reuse a product inquiry conversation")
            .Produces<ApiResponse<Guid>>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<Guid>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<Guid>>(StatusCodes.Status404NotFound);

        group.MapPost("seller-orders/{sellerOrderId:guid}", StartSellerOrderConversation)
            .WithName(nameof(StartSellerOrderConversation))
            .WithSummary("Start or reuse a seller-order conversation")
            .Produces<ApiResponse<Guid>>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<Guid>>(StatusCodes.Status404NotFound);

        group.MapGet(string.Empty, GetConversations)
            .WithName(nameof(GetConversations))
            .WithSummary("Get the current user's conversations")
            .Produces<ApiResponse<PagedListResponse<ConversationResponse>>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("{conversationId:guid}/messages", GetMessages)
            .WithName(nameof(GetMessages))
            .WithSummary("Get conversation messages")
            .Produces<ApiResponse<PagedListResponse<ConversationMessageResponse>>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<PagedListResponse<ConversationMessageResponse>>>(StatusCodes.Status404NotFound);

        group.MapPost("{conversationId:guid}/messages", SendMessage)
            .WithName(nameof(SendMessage))
            .WithSummary("Send a conversation message")
            .Produces<ApiResponse<Guid>>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<Guid>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<Guid>>(StatusCodes.Status404NotFound);

        group.MapPost("{conversationId:guid}/read", MarkRead)
            .WithName(nameof(MarkRead))
            .WithSummary("Mark a conversation as read")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound);

        return builder;
    }

    /// <summary>
    /// Starts or reuses a product inquiry.
    /// </summary>
    public static async Task<IResult> StartProductInquiry(
        Guid productId,
        ISender sender,
        ClaimsPrincipal user,
        IRequestClient<GetAccountUserIdByIdentityIdRequest> accountClient,
        CancellationToken cancellationToken)
    {
        var currentUserId = await GetCurrentUserIdAsync(user, accountClient, cancellationToken);

        if (currentUserId is null)
        {
            return Results.Forbid();
        }

        var result = await sender.Send(
            new StartProductInquiryCommand(currentUserId.Value, productId),
            cancellationToken);

        if (result.IsSuccess)
        {
            return Results.CreatedAtRoute(
                nameof(GetMessages),
                new { conversationId = result.Value, version = MessagingApiApiVersions.V1RouteValue },
                result.MapToApiResponse());
        }

        return result.Error == ConversationErrors.ProductNotFound
            ? Results.NotFound(result.MapToApiResponse())
            : Results.BadRequest(result.MapToApiResponse());
    }

    /// <summary>
    /// Starts or reuses a seller-order conversation.
    /// </summary>
    public static async Task<IResult> StartSellerOrderConversation(
        Guid sellerOrderId,
        ISender sender,
        ClaimsPrincipal user,
        IRequestClient<GetAccountUserIdByIdentityIdRequest> accountClient,
        CancellationToken cancellationToken)
    {
        var currentUserId = await GetCurrentUserIdAsync(user, accountClient, cancellationToken);

        if (currentUserId is null)
        {
            return Results.Forbid();
        }

        var result = await sender.Send(
            new StartSellerOrderConversationCommand(currentUserId.Value, sellerOrderId),
            cancellationToken);

        if (result.IsSuccess)
        {
            return Results.CreatedAtRoute(
                nameof(GetMessages),
                new { conversationId = result.Value, version = MessagingApiApiVersions.V1RouteValue },
                result.MapToApiResponse());
        }

        if (result.Error == ConversationErrors.Forbidden)
        {
            return Results.Forbid();
        }

        return Results.NotFound(result.MapToApiResponse());
    }

    /// <summary>
    /// Gets the current user's conversations.
    /// </summary>
    public static async Task<IResult> GetConversations(
        [AsParameters] ConversationPageRequest request,
        ISender sender,
        ClaimsPrincipal user,
        IRequestClient<GetAccountUserIdByIdentityIdRequest> accountClient,
        CancellationToken cancellationToken)
    {
        var currentUserId = await GetCurrentUserIdAsync(user, accountClient, cancellationToken);

        if (currentUserId is null)
        {
            return Results.Forbid();
        }

        var result = await sender.Send(
            new GetConversationsPageQuery(currentUserId.Value, request.Page, request.PageSize),
            cancellationToken);

        return Results.Ok(result.MapToApiResponse());
    }

    /// <summary>
    /// Gets messages for a conversation.
    /// </summary>
    public static async Task<IResult> GetMessages(
        Guid conversationId,
        [AsParameters] ConversationPageRequest request,
        ISender sender,
        ClaimsPrincipal user,
        IRequestClient<GetAccountUserIdByIdentityIdRequest> accountClient,
        CancellationToken cancellationToken)
    {
        var currentUserId = await GetCurrentUserIdAsync(user, accountClient, cancellationToken);

        if (currentUserId is null)
        {
            return Results.Forbid();
        }

        var result = await sender.Send(
            new GetConversationMessagesPageQuery(
                currentUserId.Value,
                conversationId,
                request.Page,
                request.PageSize),
            cancellationToken);

        if (result.IsSuccess)
        {
            return Results.Ok(result.MapToApiResponse());
        }

        return result.Error == ConversationErrors.Forbidden
            ? Results.Forbid()
            : Results.NotFound(result.MapToApiResponse());
    }

    /// <summary>
    /// Sends a message to a conversation.
    /// </summary>
    public static async Task<IResult> SendMessage(
        Guid conversationId,
        SendConversationMessageRequest request,
        ISender sender,
        ClaimsPrincipal user,
        IRequestClient<GetAccountUserIdByIdentityIdRequest> accountClient,
        CancellationToken cancellationToken)
    {
        var currentUserId = await GetCurrentUserIdAsync(user, accountClient, cancellationToken);

        if (currentUserId is null)
        {
            return Results.Forbid();
        }

        var result = await sender.Send(
            new SendConversationMessageCommand(currentUserId.Value, conversationId, request.Body),
            cancellationToken);

        if (result.IsSuccess)
        {
            return Results.CreatedAtRoute(
                nameof(GetMessages),
                new { conversationId, version = MessagingApiApiVersions.V1RouteValue },
                result.MapToApiResponse());
        }

        if (result.Error == ConversationErrors.Forbidden)
        {
            return Results.Forbid();
        }

        return result.Error == ConversationErrors.NotFound
            ? Results.NotFound(result.MapToApiResponse())
            : Results.BadRequest(result.MapToApiResponse());
    }

    /// <summary>
    /// Marks a conversation as read for the current participant.
    /// </summary>
    public static async Task<IResult> MarkRead(
        Guid conversationId,
        ISender sender,
        ClaimsPrincipal user,
        IRequestClient<GetAccountUserIdByIdentityIdRequest> accountClient,
        CancellationToken cancellationToken)
    {
        var currentUserId = await GetCurrentUserIdAsync(user, accountClient, cancellationToken);

        if (currentUserId is null)
        {
            return Results.Forbid();
        }

        var result = await sender.Send(
            new MarkConversationReadCommand(currentUserId.Value, conversationId),
            cancellationToken);

        if (result.IsSuccess)
        {
            return Results.NoContent();
        }

        return result.Error == ConversationErrors.Forbidden
            ? Results.Forbid()
            : Results.NotFound(result.MapToApiResponse());
    }

    private static async Task<Guid?> GetCurrentUserIdAsync(
        ClaimsPrincipal user,
        IRequestClient<GetAccountUserIdByIdentityIdRequest> accountClient,
        CancellationToken cancellationToken)
    {
        var identityId = user.FindFirstValue("identity_id") ??
            user.FindFirstValue("IdentityId") ??
            user.FindFirstValue(ClaimTypes.NameIdentifier) ??
            user.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(identityId))
        {
            return null;
        }

        var response = await accountClient.GetResponse<GetAccountUserIdByIdentityIdResponse>(
            new GetAccountUserIdByIdentityIdRequest(identityId),
            cancellationToken);

        return response.Message.Found
            ? response.Message.UserId
            : null;
    }
}

