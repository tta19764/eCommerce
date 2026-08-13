using MediatR;
using PaymentApi.Application.Webhooks;

namespace PaymentApi.Api.Endpoints.Webhooks;

/// <summary>Defines payment-provider webhook endpoints.</summary>
public static class WebhookEndpoints
{
    /// <summary>Maps the anonymous Stripe webhook endpoint.</summary>
    public static IEndpointRouteBuilder MapWebhookEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("webhooks/stripe", ProcessStripeWebhook)
            .WithName(nameof(ProcessStripeWebhook))
            .WithTags("Webhooks")
            .HasApiVersion(PaymentApiApiVersions.V1)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

        return builder;
    }

    /// <summary>Verifies and processes one Stripe webhook request.</summary>
    public static async Task<IResult> ProcessStripeWebhook(
        HttpRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(request.Body);
        var payload = await reader.ReadToEndAsync(cancellationToken);
        var signature = request.Headers["Stripe-Signature"].ToString();
        var result = await sender.Send(
            new ProcessStripeWebhookCommand(payload, signature),
            cancellationToken);

        return result.IsSuccess ? Results.Ok() : Results.BadRequest();
    }
}
