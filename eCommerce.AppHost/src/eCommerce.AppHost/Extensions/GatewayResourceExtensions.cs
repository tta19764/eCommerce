using eCommerce.AppHost.Configuration;

namespace eCommerce.AppHost.Extensions;

/// <summary>
/// Provides methods that add the API gateway to the application.
/// </summary>
public static class GatewayResourceExtensions
{
    /// <summary>
    /// Adds the API gateway and its backend service dependencies.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <param name="parameters">The secret application parameters.</param>
    /// <param name="settings">The application host settings.</param>
    /// <param name="services">The backend service resources.</param>
    /// <returns>The API gateway resource.</returns>
    public static IResourceBuilder<ProjectResource> AddGateway(
        this IDistributedApplicationBuilder builder,
        AppHostParameters parameters,
        AppHostSettings settings,
        BackendServiceResources services)
    {
        return builder.AddProject<Projects.GatewayApi_Api>("gateway-api")
            .WithHttpEndpoint(port: settings.GatewayApiPort)
            .WithHttpsEndpoint(port: settings.GatewayApiHttpsPort)
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", settings.ProjectEnvironment)
            .WithEnvironment("DOTNET_ENVIRONMENT", settings.ProjectEnvironment)
            .WithEnvironment("Gateway__HeaderName", settings.GatewayHeaderName)
            .WithEnvironment("Gateway__Signature", parameters.GatewaySignature)
            .WithReference(services.AuthenticationApi)
            .WithReference(services.ProductApi)
            .WithReference(services.OrderApi)
            .WithReference(services.PaymentApi)
            .WithReference(services.UserApi)
            .WithReference(services.SellerApi)
            .WithReference(services.ImageApi)
            .WithReference(services.NotificationApi)
            .WithReference(services.MessagingApi)
            .WaitFor(services.AuthenticationApi)
            .WaitFor(services.ProductApi)
            .WaitFor(services.OrderApi)
            .WaitFor(services.PaymentApi)
            .WaitFor(services.UserApi)
            .WaitFor(services.SellerApi)
            .WaitFor(services.ImageApi)
            .WaitFor(services.NotificationApi)
            .WaitFor(services.MessagingApi)
            .WithExternalHttpEndpoints();
    }
}
