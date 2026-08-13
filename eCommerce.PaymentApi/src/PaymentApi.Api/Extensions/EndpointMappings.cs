using PaymentApi.Api.Endpoints;
using PaymentApi.Api.Endpoints.Payments;
using PaymentApi.Api.Endpoints.Webhooks;

namespace PaymentApi.Api.Extensions;

/// <summary>Maps all PaymentApi endpoints.</summary>
public static class EndpointMappings
{
    /// <summary>Maps versioned payment and webhook endpoint groups.</summary>
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder)
    {
        var versionSet = builder.NewApiVersionSet()
            .HasApiVersion(PaymentApiApiVersions.V1)
            .ReportApiVersions()
            .Build();

        var api = builder.MapGroup("api/v{version:apiVersion}")
            .WithApiVersionSet(versionSet);

        api.MapPaymentEndpoints();
        api.MapWebhookEndpoints();

        return builder;
    }
}
