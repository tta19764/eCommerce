using Aspire.Hosting.ApplicationModel;
using eCommerce.AppHost.Configuration;

namespace eCommerce.AppHost.Extensions;

/// <summary>Contains the backend service project resources.</summary>
public sealed record BackendServiceResources(
    IResourceBuilder<ProjectResource> AuthenticationApi,
    IResourceBuilder<ProjectResource> ProductApi,
    IResourceBuilder<ProjectResource> OrderApi,
    IResourceBuilder<ProjectResource> PaymentApi,
    IResourceBuilder<ProjectResource> UserApi,
    IResourceBuilder<ProjectResource> SellerApi,
    IResourceBuilder<ProjectResource> ImageApi,
    IResourceBuilder<ProjectResource> NotificationApi,
    IResourceBuilder<ProjectResource> MessagingApi);

/// <summary>Adds the backend service projects.</summary>
public static class BackendServiceExtensions
{
    /// <summary>Adds all backend APIs and their infrastructure dependencies.</summary>
    public static BackendServiceResources AddBackendServices(this IDistributedApplicationBuilder builder, AppHostParameters parameters, AppHostSettings settings, InfrastructureResources infrastructure)
    {
        // AuthenticationApi owns identity/account workflows. It depends on PostgreSQL for local account
        // state, RabbitMQ for profile creation requests to UserApi, Keycloak for real identity management,
        // and Seq for centralized logs.
        var authenticationApi = builder.AddProject<Projects.AuthenticationApi_Api>("authentication-api")
            .WithHttpEndpoint(port: settings.AuthenticationApiPort)
            .WithHttpsEndpoint(port: settings.AuthenticationApiHttpsPort)
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", settings.ProjectEnvironment)
            .WithEnvironment("DOTNET_ENVIRONMENT", settings.ProjectEnvironment)
            .WithEnvironment("Gateway__HeaderName", settings.GatewayHeaderName)
            .WithEnvironment("Gateway__Signature", parameters.GatewaySignature)
            .WithEnvironment("Serilog__WriteTo__1__Name", settings.SeqSinkName)
            .WithEnvironment("Serilog__WriteTo__1__Args__serverUrl", settings.SeqServerUrl)
            .WithEnvironment("ConnectionStrings__Redis", settings.RedisConnectionString)
            .WithReference(infrastructure.AuthenticationDb, "Database")
            .WithReference(infrastructure.RabbitMq)
            .WithEnvironment("Authentication__Audience", settings.AuthenticationAudience)
            .WithEnvironment("Authentication__MetadataUrl", settings.AuthenticationMetadataUrl)
            .WithEnvironment("Authentication__RequireHttpsMetadata", settings.AuthenticationRequireHttpsMetadata)
            .WithEnvironment("Authentication__Issuer", settings.AuthenticationIssuer)
            .WithEnvironment("Keycloak__AdminUrl", settings.KeycloakAdminUrl)
            .WithEnvironment("Keycloak__TokenUrl", settings.KeycloakTokenUrl)
            .WithEnvironment("Keycloak__AdminClientId", settings.KeycloakAdminClientId)
            .WithEnvironment("Keycloak__AdminClientSecret", parameters.KeycloakAdminClientSecret)
            .WithEnvironment("Keycloak__AuthClientId", settings.KeycloakAuthClientId)
            .WithEnvironment("Keycloak__AuthClientSecret", parameters.KeycloakAuthClientSecret)
            .WithEnvironment("BootstrapAdmin__Enabled", settings.BootstrapAdminEnabled)
            .WithEnvironment("BootstrapAdmin__Email", settings.BootstrapAdminEmail)
            .WithEnvironment("BootstrapAdmin__Password", parameters.BootstrapAdminPassword)
            .WithEnvironment("BootstrapAdmin__FirstName", settings.BootstrapAdminFirstName)
            .WithEnvironment("BootstrapAdmin__LastName", settings.BootstrapAdminLastName)
            .WaitFor(infrastructure.Postgres)
            .WaitFor(infrastructure.RabbitMq)
            .WaitFor(infrastructure.Keycloak)
            .WaitFor(infrastructure.Redis)
            .WaitFor(infrastructure.Seq);

        // ProductApi owns catalog data and publishes/serves product-related messages through RabbitMQ.
        // Gateway settings are injected even when testing locally so direct service access remains blocked
        // in the same way it is expected to be blocked behind the gateway.
        var productApi = builder.AddProject<Projects.ProductApi_Api>("product-api")
            .WithHttpEndpoint(port: settings.ProductApiPort)
            .WithHttpsEndpoint(port: settings.ProductApiHttpsPort)
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", settings.ProjectEnvironment)
            .WithEnvironment("DOTNET_ENVIRONMENT", settings.ProjectEnvironment)
            .WithEnvironment("Gateway__HeaderName", settings.GatewayHeaderName)
            .WithEnvironment("Gateway__Signature", parameters.GatewaySignature)
            .WithEnvironment("Serilog__WriteTo__1__Name", settings.SeqSinkName)
            .WithEnvironment("Serilog__WriteTo__1__Args__serverUrl", settings.SeqServerUrl)
            .WithEnvironment("ConnectionStrings__Redis", settings.RedisConnectionString)
            .WithEnvironment("Authentication__Audience", settings.AuthenticationAudience)
            .WithEnvironment("Authentication__MetadataUrl", settings.AuthenticationMetadataUrl)
            .WithEnvironment("Authentication__RequireHttpsMetadata", settings.AuthenticationRequireHttpsMetadata)
            .WithEnvironment("Authentication__Issuer", settings.AuthenticationIssuer)
            .WithReference(infrastructure.ProductDb, "Database")
            .WithReference(infrastructure.RabbitMq)
            .WaitFor(infrastructure.Postgres)
            .WaitFor(infrastructure.RabbitMq)
            .WaitFor(infrastructure.Redis)
            .WaitFor(infrastructure.Seq);

        // OrderApi owns order state. It depends on RabbitMQ because order workflows call other services for
        // product/user data, and it gets its own PostgreSQL database so order schema changes stay isolated.
        var orderApi = builder.AddProject<Projects.OrderApi_Api>("order-api")
            .WithHttpEndpoint(port: settings.OrderApiPort)
            .WithHttpsEndpoint(port: settings.OrderApiHttpsPort)
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", settings.ProjectEnvironment)
            .WithEnvironment("DOTNET_ENVIRONMENT", settings.ProjectEnvironment)
            .WithEnvironment("Gateway__HeaderName", settings.GatewayHeaderName)
            .WithEnvironment("Gateway__Signature", parameters.GatewaySignature)
            .WithEnvironment("Serilog__WriteTo__1__Name", settings.SeqSinkName)
            .WithEnvironment("Serilog__WriteTo__1__Args__serverUrl", settings.SeqServerUrl)
            .WithEnvironment("ConnectionStrings__Redis", settings.RedisConnectionString)
            .WithEnvironment("Authentication__Audience", settings.AuthenticationAudience)
            .WithEnvironment("Authentication__MetadataUrl", settings.AuthenticationMetadataUrl)
            .WithEnvironment("Authentication__RequireHttpsMetadata", settings.AuthenticationRequireHttpsMetadata)
            .WithEnvironment("Authentication__Issuer", settings.AuthenticationIssuer)
            .WithReference(infrastructure.OrderDb, "Database")
            .WithReference(infrastructure.RabbitMq)
            .WaitFor(infrastructure.Postgres)
            .WaitFor(infrastructure.RabbitMq)
            .WaitFor(infrastructure.Redis)
            .WaitFor(infrastructure.Seq);

        // PaymentApi owns Stripe payment state, verified webhook receipts, and payment integration events.
        var paymentApi = builder.AddProject<Projects.PaymentApi_Api>("payment-api")
            .WithHttpEndpoint(port: settings.PaymentApiPort)
            .WithHttpsEndpoint(port: settings.PaymentApiHttpsPort)
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", settings.ProjectEnvironment)
            .WithEnvironment("DOTNET_ENVIRONMENT", settings.ProjectEnvironment)
            .WithEnvironment("Gateway__HeaderName", settings.GatewayHeaderName)
            .WithEnvironment("Gateway__Signature", parameters.GatewaySignature)
            .WithEnvironment("Stripe__SecretKey", parameters.StripeSecretKey)
            .WithEnvironment("Stripe__PublishableKey", parameters.StripePublishableKey)
            .WithEnvironment("Stripe__WebhookSecret", parameters.StripeWebhookSecret)
            .WithEnvironment("Authentication__Audience", settings.AuthenticationAudience)
            .WithEnvironment("Authentication__MetadataUrl", settings.AuthenticationMetadataUrl)
            .WithEnvironment("Authentication__RequireHttpsMetadata", settings.AuthenticationRequireHttpsMetadata)
            .WithEnvironment("Authentication__Issuer", settings.AuthenticationIssuer)
            .WithEnvironment("Serilog__WriteTo__1__Name", settings.SeqSinkName)
            .WithEnvironment("Serilog__WriteTo__1__Args__serverUrl", settings.SeqServerUrl)
            .WithReference(infrastructure.PaymentDb, "Database")
            .WithReference(infrastructure.RabbitMq)
            .WaitFor(infrastructure.Postgres)
            .WaitFor(infrastructure.RabbitMq)
            .WaitFor(orderApi)
            .WaitFor(infrastructure.Seq);

        // UserApi owns profile data linked from AuthenticationApi accounts. AuthenticationApi creates and
        // deletes profiles through MassTransit, so UserApi must wait for RabbitMQ and PostgreSQL before it
        // can reliably process profile messages.
        var userApi = builder.AddProject<Projects.UserApi_Api>("user-api")
            .WithHttpEndpoint(port: settings.UserApiPort)
            .WithHttpsEndpoint(port: settings.UserApiHttpsPort)
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", settings.ProjectEnvironment)
            .WithEnvironment("DOTNET_ENVIRONMENT", settings.ProjectEnvironment)
            .WithEnvironment("Gateway__HeaderName", settings.GatewayHeaderName)
            .WithEnvironment("Gateway__Signature", parameters.GatewaySignature)
            .WithEnvironment("Serilog__WriteTo__1__Name", settings.SeqSinkName)
            .WithEnvironment("Serilog__WriteTo__1__Args__serverUrl", settings.SeqServerUrl)
            .WithEnvironment("Authentication__Audience", settings.AuthenticationAudience)
            .WithEnvironment("Authentication__MetadataUrl", settings.AuthenticationMetadataUrl)
            .WithEnvironment("Authentication__RequireHttpsMetadata", settings.AuthenticationRequireHttpsMetadata)
            .WithEnvironment("Authentication__Issuer", settings.AuthenticationIssuer)
            .WithReference(infrastructure.UserDb, "Database")
            .WithReference(infrastructure.RabbitMq)
            .WaitFor(infrastructure.Postgres)
            .WaitFor(infrastructure.RabbitMq)
            .WaitFor(infrastructure.Seq);

        var sellerApi = builder.AddProject<Projects.SellerApi_Api>("seller-api")
            .WithHttpEndpoint(port: settings.SellerApiPort)
            .WithHttpsEndpoint(port: settings.SellerApiHttpsPort)
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", settings.ProjectEnvironment)
            .WithEnvironment("DOTNET_ENVIRONMENT", settings.ProjectEnvironment)
            .WithEnvironment("Gateway__HeaderName", settings.GatewayHeaderName)
            .WithEnvironment("Gateway__Signature", parameters.GatewaySignature)
            .WithEnvironment("Authentication__Audience", settings.AuthenticationAudience)
            .WithEnvironment("Authentication__MetadataUrl", settings.AuthenticationMetadataUrl)
            .WithEnvironment("Authentication__RequireHttpsMetadata", settings.AuthenticationRequireHttpsMetadata)
            .WithEnvironment("Authentication__Issuer", settings.AuthenticationIssuer)
            .WithReference(infrastructure.SellerDb, "Database")
            .WithReference(infrastructure.RabbitMq)
            .WaitFor(infrastructure.Postgres)
            .WaitFor(infrastructure.RabbitMq)
            .WaitFor(orderApi)
            .WaitFor(infrastructure.Seq);

        // ImageApi owns image metadata in PostgreSQL and binary object storage in MinIO. It also listens on
        // RabbitMQ for image-reference validation requests from services that store ImageId values.
        var imageApi = builder.AddProject<Projects.ImageApi_Api>("image-api")
            .WithHttpEndpoint(port: settings.ImageApiPort)
            .WithHttpsEndpoint(port: settings.ImageApiHttpsPort)
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", settings.ProjectEnvironment)
            .WithEnvironment("DOTNET_ENVIRONMENT", settings.ProjectEnvironment)
            .WithEnvironment("Gateway__HeaderName", settings.GatewayHeaderName)
            .WithEnvironment("Gateway__Signature", parameters.GatewaySignature)
            .WithEnvironment("Serilog__WriteTo__1__Name", settings.SeqSinkName)
            .WithEnvironment("Serilog__WriteTo__1__Args__serverUrl", settings.SeqServerUrl)
            .WithEnvironment("Authentication__Audience", settings.AuthenticationAudience)
            .WithEnvironment("Authentication__MetadataUrl", settings.AuthenticationMetadataUrl)
            .WithEnvironment("Authentication__RequireHttpsMetadata", settings.AuthenticationRequireHttpsMetadata)
            .WithEnvironment("Authentication__Issuer", settings.AuthenticationIssuer)
            .WithEnvironment("S3Storage__ServiceUrl", settings.MinioServiceUrl)
            .WithEnvironment("S3Storage__AccessKey", parameters.MinioRootUser)
            .WithEnvironment("S3Storage__SecretKey", parameters.MinioRootPassword)
            .WithEnvironment("S3Storage__BucketName", settings.MinioBucketName)
            .WithEnvironment("S3Storage__Region", settings.MinioRegion)
            .WithEnvironment("S3Storage__ForcePathStyle", settings.MinioForcePathStyle)
            .WithEnvironment("S3Storage__PresignedUrlExpiryMinutes", settings.MinioPresignedUrlExpiryMinutes)
            .WithReference(infrastructure.ImageDb, "Database")
            .WithReference(infrastructure.RabbitMq)
            .WaitFor(infrastructure.Minio)
            .WaitFor(infrastructure.Postgres)
            .WaitFor(infrastructure.RabbitMq)
            .WaitFor(infrastructure.Seq);

        // NotificationApi owns durable notification jobs. Other services publish notification requests to
        // RabbitMQ, this service stores them in its PostgreSQL database, and its hosted worker retries
        // delivery independently of the user-facing request that created the job.
        var notificationApi = builder.AddProject<Projects.NotificationApi_Api>("notification-api")
            .WithHttpEndpoint(port: settings.NotificationApiPort)
            .WithHttpsEndpoint(port: settings.NotificationApiHttpsPort)
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", settings.ProjectEnvironment)
            .WithEnvironment("DOTNET_ENVIRONMENT", settings.ProjectEnvironment)
            .WithEnvironment("Serilog__WriteTo__1__Name", settings.SeqSinkName)
            .WithEnvironment("Serilog__WriteTo__1__Args__serverUrl", settings.SeqServerUrl)
            .WithEnvironment("Email__FromAddress", parameters.NotificationFromAddress)
            .WithEnvironment("Email__EmailConfirmationUrlTemplate", settings.NotificationEmailConfirmationUrlTemplate)
            .WithEnvironment("Smtp__Host", settings.NotificationSmtpHost)
            .WithEnvironment("Smtp__Port", settings.NotificationSmtpPort)
            .WithEnvironment("Smtp__EnableSsl", settings.NotificationSmtpEnableSsl)
            .WithEnvironment("Smtp__FromName", settings.NotificationSmtpFromName)
            .WithEnvironment("Smtp__TimeoutSeconds", settings.NotificationSmtpTimeoutSeconds)
            .WithEnvironment("Smtp__UserName", parameters.NotificationSmtpUserName)
            .WithEnvironment("Smtp__Password", parameters.NotificationSmtpPassword)
            .WithReference(infrastructure.NotificationDb, "Database")
            .WithReference(infrastructure.RabbitMq)
            .WaitFor(infrastructure.Postgres)
            .WaitFor(infrastructure.RabbitMq)
            .WaitFor(infrastructure.Mailpit)
            .WaitFor(infrastructure.Seq);

        // MessagingApi owns marketplace conversations between customers and sellers. It uses PostgreSQL
        // for durable chat history and RabbitMQ to validate product/order participants with the owning
        // services before creating conversations.
        var messagingApi = builder.AddProject<Projects.MessagingApi_Api>("messaging-api")
            .WithHttpEndpoint(port: settings.MessagingApiPort)
            .WithHttpsEndpoint(port: settings.MessagingApiHttpsPort)
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", settings.ProjectEnvironment)
            .WithEnvironment("DOTNET_ENVIRONMENT", settings.ProjectEnvironment)
            .WithEnvironment("Gateway__HeaderName", settings.GatewayHeaderName)
            .WithEnvironment("Gateway__Signature", parameters.GatewaySignature)
            .WithEnvironment("Serilog__WriteTo__1__Name", settings.SeqSinkName)
            .WithEnvironment("Serilog__WriteTo__1__Args__serverUrl", settings.SeqServerUrl)
            .WithEnvironment("Authentication__Audience", settings.AuthenticationAudience)
            .WithEnvironment("Authentication__MetadataUrl", settings.AuthenticationMetadataUrl)
            .WithEnvironment("Authentication__RequireHttpsMetadata", settings.AuthenticationRequireHttpsMetadata)
            .WithEnvironment("Authentication__Issuer", settings.AuthenticationIssuer)
            .WithReference(infrastructure.MessagingDb, "Database")
            .WithReference(infrastructure.RabbitMq)
            .WaitFor(infrastructure.Postgres)
            .WaitFor(infrastructure.RabbitMq)
            .WaitFor(infrastructure.Seq);

        return new BackendServiceResources(authenticationApi, productApi, orderApi, paymentApi, userApi, sellerApi, imageApi, notificationApi, messagingApi);
    }
}
