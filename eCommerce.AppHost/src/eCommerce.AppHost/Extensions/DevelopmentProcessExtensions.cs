using Aspire.Hosting.ApplicationModel;
using eCommerce.AppHost.Configuration;

namespace eCommerce.AppHost.Extensions;

/// <summary>Adds local development processes to AppHost.</summary>
public static class DevelopmentProcessExtensions
{
    /// <summary>Adds the Stripe listener and Angular development server.</summary>
    public static IDistributedApplicationBuilder AddDevelopmentProcesses(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<ProjectResource> gatewayApi,
        AppHostSettings settings)
    {
        // Wait for the Gateway because webhooks must traverse its signature boundary. --skip-verify applies
        // only to the local development certificate used by the HTTPS forwarding target.
        builder.AddExecutable(
                "stripe-listener",
                settings.StripeCliCommand,
                builder.AppHostDirectory,
                "listen",
                "--forward-to",
                settings.StripeCliForwardTo,
                "--events",
                settings.StripeCliEvents,
                "--skip-verify")
            .WaitFor(gatewayApi);

        // WebApp runs Angular's development server as a normal local process. Install packages once in the
        // Angular project with npm install/npm ci, then AppHost can start and supervise the dev server.
        builder.AddExecutable(
                "web-app",
                settings.WebAppCommand,
                settings.WebAppSourcePath,
                "start",
                "--",
                "--host",
                "localhost",
                "--port",
                settings.WebAppPort.ToString())
            .WithHttpEndpoint(port: settings.WebAppPort, targetPort: settings.WebAppPort, name: "http", isProxied: false)
            .WithEnvironment("NG_CLI_ANALYTICS", "false")
            .WaitFor(gatewayApi)
            .WithExternalHttpEndpoints();


        return builder;
    }
}
