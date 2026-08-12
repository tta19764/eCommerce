
using eCommerce.AppHost.Configuration;

namespace eCommerce.AppHost.Extensions;

/// <summary>
/// Adds the local infrastructure, services, gateway, and development processes.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds the complete eCommerce resource graph to the distributed application.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <returns>The configured distributed application builder.</returns>
    public static IDistributedApplicationBuilder AddEcommerceResources(this IDistributedApplicationBuilder builder)
    {
        var parameters = builder.AddAppHostParameters();

        var settings = AppHostSettings.Load(builder);

        var infrastructure = builder.AddInfrastructureResources(parameters, settings);

        var services = builder.AddBackendServices(parameters, settings, infrastructure);

        var gatewayApi = builder.AddGateway(parameters, settings, services);

        builder.AddDevelopmentProcesses(gatewayApi, settings);

        return builder;
    }
}
