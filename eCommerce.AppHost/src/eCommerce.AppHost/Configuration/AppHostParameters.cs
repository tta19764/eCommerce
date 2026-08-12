using Aspire.Hosting.ApplicationModel;

namespace eCommerce.AppHost.Configuration;

/// <summary>Contains the external parameters used by AppHost resources.</summary>
public sealed record AppHostParameters(
    IResourceBuilder<ParameterResource> PostgresUser,
    IResourceBuilder<ParameterResource> PostgresPassword,
    IResourceBuilder<ParameterResource> PgAdminEmail,
    IResourceBuilder<ParameterResource> PgAdminPassword,
    IResourceBuilder<ParameterResource> RabbitMqUser,
    IResourceBuilder<ParameterResource> RabbitMqPassword,
    IResourceBuilder<ParameterResource> GatewaySignature,
    IResourceBuilder<ParameterResource> StripeSecretKey,
    IResourceBuilder<ParameterResource> StripePublishableKey,
    IResourceBuilder<ParameterResource> StripeWebhookSecret,
    IResourceBuilder<ParameterResource> MinioRootUser,
    IResourceBuilder<ParameterResource> MinioRootPassword,
    IResourceBuilder<ParameterResource> KeycloakAdminUser,
    IResourceBuilder<ParameterResource> KeycloakAdminPassword,
    IResourceBuilder<ParameterResource> KeycloakAdminClientSecret,
    IResourceBuilder<ParameterResource> KeycloakAuthClientSecret,
    IResourceBuilder<ParameterResource> BootstrapAdminPassword,
    IResourceBuilder<ParameterResource> NotificationFromAddress,
    IResourceBuilder<ParameterResource> NotificationSmtpUserName,
    IResourceBuilder<ParameterResource> NotificationSmtpPassword);

/// <summary>Registers external AppHost parameters.</summary>
public static class ParameterExtensions
{
    /// <summary>Adds public and secret parameters to the application model.</summary>
    public static AppHostParameters AddAppHostParameters(this IDistributedApplicationBuilder builder)
    {
        return new AppHostParameters(
            builder.AddParameter("postgres-user"),
            builder.AddParameter("postgres-password", secret: true),
            builder.AddParameter("pgadmin-email"),
            builder.AddParameter("pgadmin-password", secret: true),
            builder.AddParameter("rabbitmq-user"),
            builder.AddParameter("rabbitmq-password", secret: true),
            builder.AddParameter("gateway-signature", secret: true),
            builder.AddParameter("stripe-secret-key", secret: true),
            builder.AddParameter("stripe-publishable-key"),
            builder.AddParameter("stripe-webhook-secret", secret: true),
            builder.AddParameter("minio-root-user"),
            builder.AddParameter("minio-root-password", secret: true),
            builder.AddParameter("keycloak-admin-user"),
            builder.AddParameter("keycloak-admin-password", secret: true),
            builder.AddParameter("keycloak-admin-client-secret", secret: true),
            builder.AddParameter("keycloak-auth-client-secret", secret: true),
            builder.AddParameter("bootstrap-admin-password", secret: true),
            builder.AddParameter("notification-from-address"),
            builder.AddParameter("notification-smtp-user-name"),
            builder.AddParameter("notification-smtp-password", secret: true));
    }
}
