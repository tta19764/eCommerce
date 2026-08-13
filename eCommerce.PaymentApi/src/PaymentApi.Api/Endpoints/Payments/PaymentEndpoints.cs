using System.Security.Claims;
using AuthenticationApi.Messages.Accounts;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Options;
using PaymentApi.Application.Payments.CreatePayment;
using PaymentApi.Application.Payments.GetPayment;
using PaymentApi.Infrastructure.Stripe;
using SharedLibrary.Api.Contracts;
using SharedLibrary.Api.Extensions;

namespace PaymentApi.Api.Endpoints.Payments;

/// <summary>Defines authenticated payment endpoints.</summary>
public static class PaymentEndpoints
{
    /// <summary>Maps payment creation, query, and configuration endpoints.</summary>
    public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("payments")
            .WithTags("Payments")
            .HasApiVersion(PaymentApiApiVersions.V1)
            .RequireAuthorization();

        group.MapPost(string.Empty, CreatePayment)
            .WithName(nameof(CreatePayment))
            .Produces<ApiResponse<CreatePaymentResponse>>()
            .Produces<ApiResponse<CreatePaymentResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("{paymentId:guid}", GetPayment)
            .WithName(nameof(GetPayment))
            .Produces<ApiResponse<PaymentResponse>>()
            .Produces<ApiResponse<PaymentResponse>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("config", GetConfig)
            .WithName(nameof(GetConfig))
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        return builder;
    }

    /// <summary>Creates or reuses a payment for the current customer.</summary>
    public static async Task<IResult> CreatePayment(
        CreatePaymentRequest request,
        ISender sender,
        IRequestClient<GetAccountUserIdByIdentityIdRequest> accountClient,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var customerId = await user.GetCurrentUserIdAsync(accountClient, cancellationToken);
        if (customerId is null)
        {
            return Results.Forbid();
        }

        var result = await sender.Send(
            new CreatePaymentCommand(request.OrderId, customerId.Value),
            cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.MapToApiResponse())
            : Results.BadRequest(result.MapToApiResponse());
    }

    /// <summary>Gets one payment owned by the current customer.</summary>
    public static async Task<IResult> GetPayment(
        Guid paymentId,
        ISender sender,
        IRequestClient<GetAccountUserIdByIdentityIdRequest> accountClient,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var customerId = await user.GetCurrentUserIdAsync(accountClient, cancellationToken);
        if (customerId is null)
        {
            return Results.Forbid();
        }

        var result = await sender.Send(
            new GetPaymentQuery(paymentId, customerId.Value),
            cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.MapToApiResponse())
            : Results.NotFound(result.MapToApiResponse());
    }

    /// <summary>Gets public Stripe configuration for the authenticated checkout client.</summary>
    public static IResult GetConfig(IOptions<StripeOptions> options) =>
        Results.Ok(new { publishableKey = options.Value.PublishableKey });
}
