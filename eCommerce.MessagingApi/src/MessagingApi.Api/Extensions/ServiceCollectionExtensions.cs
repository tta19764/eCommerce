using Asp.Versioning;
using MessagingApi.Api.Endpoints;
using MessagingApi.Api.Realtime;
using MessagingApi.Application.Abstractions.Realtime;

namespace MessagingApi.Api.Extensions;

/// <summary>
/// Registers API-layer services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds OpenAPI and URL-segment API versioning.
    /// </summary>
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddOpenApi();
        services.AddEndpointsApiExplorer();

        services.AddSignalR();
        services.AddSingleton<IConversationsRealtimeNotifier, SignalRConversationsRealtimeNotifier>();

        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = MessagingApiApiVersions.V1;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl = true;
            });

        return services;
    }
}

