using System.Security.Claims;
using Asp.Versioning;
using AuthenticationApi.Messages.Accounts;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Options;
using PaymentApi.Application.Payments.CreatePayment;
using PaymentApi.Application.Payments.GetPayment;
using PaymentApi.Application.Webhooks;
using PaymentApi.Infrastructure.Stripe;
using SharedLibrary.Api.Extensions;

namespace PaymentApi.Api;

/// <summary>Registers and maps PaymentApi's authenticated customer endpoints and Stripe webhook boundary.</summary>
public static class PaymentApiExtensions
{
    /// <summary>Registers OpenAPI and URL-segment API versioning for PaymentApi.</summary>
    public static IServiceCollection AddPaymentApi(this IServiceCollection services)
    {
        services.AddOpenApi();
        services.AddEndpointsApiExplorer();
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        });
        return services;
    }

    /// <summary>Maps payment creation/query/configuration routes and the signature-verified webhook route.</summary>
    public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var versions = endpoints.NewApiVersionSet().HasApiVersion(new ApiVersion(1, 0)).ReportApiVersions().Build();
        var api = endpoints.MapGroup("api/v{version:apiVersion}").WithApiVersionSet(versions);

        api.MapPost("payments", CreatePayment).RequireAuthorization();
        api.MapGet("payments/{paymentId:guid}", GetPayment).RequireAuthorization();
        api.MapGet("payments/config", GetConfig).RequireAuthorization();
        // The webhook cannot use bearer authentication because Stripe is the caller. Authenticity is
        // instead established from the raw body and Stripe-Signature inside the provider adapter.
        api.MapPost("webhooks/stripe", ProcessStripeWebhook).AllowAnonymous();
        return endpoints;
    }

    private static async Task<IResult> CreatePayment(
        CreatePaymentRequest request,
        ISender sender,
        IRequestClient<GetAccountUserIdByIdentityIdRequest> accountClient,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var customerId = await user.GetCurrentUserIdAsync(accountClient, cancellationToken);
        if (customerId is null) return Results.Forbid();

        var result = await sender.Send(new CreatePaymentCommand(request.OrderId, customerId.Value), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.MapToApiResponse()) : Results.BadRequest(result.MapToApiResponse());
    }

    private static async Task<IResult> GetPayment(
        Guid paymentId,
        ISender sender,
        IRequestClient<GetAccountUserIdByIdentityIdRequest> accountClient,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var customerId = await user.GetCurrentUserIdAsync(accountClient, cancellationToken);
        if (customerId is null) return Results.Forbid();

        var result = await sender.Send(new GetPaymentQuery(paymentId, customerId.Value), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.MapToApiResponse()) : Results.NotFound(result.MapToApiResponse());
    }

    private static IResult GetConfig(IOptions<StripeOptions> options) =>
        Results.Ok(new { publishableKey = options.Value.PublishableKey });

    private static async Task<IResult> ProcessStripeWebhook(
        HttpRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(request.Body);
        var payload = await reader.ReadToEndAsync(cancellationToken);
        var signature = request.Headers["Stripe-Signature"].ToString();
        var result = await sender.Send(new ProcessStripeWebhookCommand(payload, signature), cancellationToken);
        return result.IsSuccess ? Results.Ok() : Results.BadRequest();
    }
}

/// <summary>Customer request containing only the order identity; payable money is resolved server-side.</summary>
public sealed record CreatePaymentRequest(Guid OrderId);
