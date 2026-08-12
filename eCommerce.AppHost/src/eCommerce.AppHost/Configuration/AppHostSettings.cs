namespace eCommerce.AppHost.Configuration;

/// <summary>Contains validated settings used to compose AppHost resources.</summary>
public sealed class AppHostSettings
{
    public required int PostgresPort { get; init; }

    public required string PostgresImageTag { get; init; }

    public required string PostgresDataVolume { get; init; }

    public required string PgAdminImage { get; init; }

    public required int PgAdminPort { get; init; }

    public required string PgAdminDataVolume { get; init; }

    public required string PgAdminServersPath { get; init; }

    public required string PgAdminServerMode { get; init; }

    public required string PgAdminMasterPasswordRequired { get; init; }

    public required string ProductDbName { get; init; }

    public required string OrderDbName { get; init; }

    public required string PaymentDbName { get; init; }

    public required string UserDbName { get; init; }

    public required string ImageDbName { get; init; }

    public required string AuthenticationDbName { get; init; }

    public required string NotificationDbName { get; init; }

    public required string MessagingDbName { get; init; }

    public required string SellerDbName { get; init; }

    public required int RabbitMqPort { get; init; }

    public required string RabbitMqDataVolume { get; init; }

    public required string GatewayHeaderName { get; init; }

    public required string MinioImage { get; init; }

    public required int MinioApiPort { get; init; }

    public required int MinioConsolePort { get; init; }

    public required string MinioDataVolume { get; init; }

    public required string MinioServiceUrl { get; init; }

    public required string MinioBucketName { get; init; }

    public required string MinioRegion { get; init; }

    public required string MinioForcePathStyle { get; init; }

    public required string MinioPresignedUrlExpiryMinutes { get; init; }

    public required string KeycloakImage { get; init; }

    public required int KeycloakPort { get; init; }

    public required string KeycloakHostname { get; init; }

    public required string KeycloakDataVolume { get; init; }

    public required string SeqImage { get; init; }

    public required int SeqPort { get; init; }

    public required string SeqDataVolume { get; init; }

    public required string SeqSinkName { get; init; }

    public required string SeqServerUrl { get; init; }

    public required string RedisImage { get; init; }

    public required int RedisPort { get; init; }

    public required string RedisDataVolume { get; init; }

    public required string RedisConnectionString { get; init; }

    public required string MailpitImage { get; init; }

    public required int MailpitSmtpPort { get; init; }

    public required int MailpitUiPort { get; init; }

    public required string WebAppCommand { get; init; }

    public required string WebAppSourcePath { get; init; }

    public required int WebAppPort { get; init; }

    public required string StripeCliCommand { get; init; }

    public required string StripeCliForwardTo { get; init; }

    public required string StripeCliEvents { get; init; }

    public required string AuthenticationAudience { get; init; }

    public required string AuthenticationMetadataUrl { get; init; }

    public required string AuthenticationRequireHttpsMetadata { get; init; }

    public required string AuthenticationIssuer { get; init; }

    public required string KeycloakAdminUrl { get; init; }

    public required string KeycloakTokenUrl { get; init; }

    public required string KeycloakAdminClientId { get; init; }

    public required string KeycloakAuthClientId { get; init; }

    public required string BootstrapAdminEnabled { get; init; }

    public required string BootstrapAdminEmail { get; init; }

    public required string BootstrapAdminFirstName { get; init; }

    public required string BootstrapAdminLastName { get; init; }

    public required string NotificationEmailConfirmationUrlTemplate { get; init; }

    public required string NotificationSmtpHost { get; init; }

    public required string NotificationSmtpPort { get; init; }

    public required string NotificationSmtpEnableSsl { get; init; }

    public required string NotificationSmtpFromName { get; init; }

    public required string NotificationSmtpTimeoutSeconds { get; init; }

    public required string ProjectEnvironment { get; init; }

    public required int AuthenticationApiPort { get; init; }

    public required int AuthenticationApiHttpsPort { get; init; }

    public required int ProductApiPort { get; init; }

    public required int ProductApiHttpsPort { get; init; }

    public required int OrderApiPort { get; init; }

    public required int OrderApiHttpsPort { get; init; }

    public required int PaymentApiPort { get; init; }

    public required int PaymentApiHttpsPort { get; init; }

    public required int UserApiPort { get; init; }

    public required int UserApiHttpsPort { get; init; }

    public required int SellerApiPort { get; init; }

    public required int SellerApiHttpsPort { get; init; }

    public required int ImageApiPort { get; init; }

    public required int ImageApiHttpsPort { get; init; }

    public required int NotificationApiPort { get; init; }

    public required int NotificationApiHttpsPort { get; init; }

    public required int MessagingApiPort { get; init; }

    public required int MessagingApiHttpsPort { get; init; }

    public required int GatewayApiPort { get; init; }

    public required int GatewayApiHttpsPort { get; init; }

    /// <summary>Loads and validates all required AppHost settings.</summary>
    public static AppHostSettings Load(IDistributedApplicationBuilder builder)
    {
        return new AppHostSettings
        {
            PostgresPort = builder.Configuration.GetRequiredInt("AppHost:Postgres:Port"),
            PostgresImageTag = builder.Configuration.GetRequired("AppHost:Postgres:ImageTag"),
            PostgresDataVolume = builder.Configuration.GetRequired("AppHost:Postgres:DataVolume"),
            PgAdminImage = builder.Configuration.GetRequired("AppHost:PgAdmin:Image"),
            PgAdminPort = builder.Configuration.GetRequiredInt("AppHost:PgAdmin:Port"),
            PgAdminDataVolume = builder.Configuration.GetRequired("AppHost:PgAdmin:DataVolume"),
            PgAdminServersPath = Path.GetFullPath(builder.Configuration.GetRequired("AppHost:PgAdmin:ServersPath"), builder.AppHostDirectory),
            PgAdminServerMode = builder.Configuration.GetRequired("AppHost:PgAdmin:ServerMode"),
            PgAdminMasterPasswordRequired = builder.Configuration.GetRequired("AppHost:PgAdmin:MasterPasswordRequired"),
            ProductDbName = builder.Configuration.GetRequired("AppHost:Postgres:Databases:Product"),
            OrderDbName = builder.Configuration.GetRequired("AppHost:Postgres:Databases:Order"),
            PaymentDbName = builder.Configuration.GetRequired("AppHost:Postgres:Databases:Payment"),
            UserDbName = builder.Configuration.GetRequired("AppHost:Postgres:Databases:User"),
            ImageDbName = builder.Configuration.GetRequired("AppHost:Postgres:Databases:Image"),
            AuthenticationDbName = builder.Configuration.GetRequired("AppHost:Postgres:Databases:Authentication"),
            NotificationDbName = builder.Configuration.GetRequired("AppHost:Postgres:Databases:Notification"),
            MessagingDbName = builder.Configuration.GetRequired("AppHost:Postgres:Databases:Messaging"),
            SellerDbName = builder.Configuration.GetRequired("AppHost:Postgres:Databases:Seller"),
            RabbitMqPort = builder.Configuration.GetRequiredInt("AppHost:RabbitMq:Port"),
            RabbitMqDataVolume = builder.Configuration.GetRequired("AppHost:RabbitMq:DataVolume"),
            GatewayHeaderName = builder.Configuration.GetRequired("AppHost:Gateway:HeaderName"),
            MinioImage = builder.Configuration.GetRequired("AppHost:Minio:Image"),
            MinioApiPort = builder.Configuration.GetRequiredInt("AppHost:Minio:ApiPort"),
            MinioConsolePort = builder.Configuration.GetRequiredInt("AppHost:Minio:ConsolePort"),
            MinioDataVolume = builder.Configuration.GetRequired("AppHost:Minio:DataVolume"),
            MinioServiceUrl = builder.Configuration.GetRequired("AppHost:Minio:ServiceUrl"),
            MinioBucketName = builder.Configuration.GetRequired("AppHost:Minio:BucketName"),
            MinioRegion = builder.Configuration.GetRequired("AppHost:Minio:Region"),
            MinioForcePathStyle = builder.Configuration.GetRequired("AppHost:Minio:ForcePathStyle"),
            MinioPresignedUrlExpiryMinutes = builder.Configuration.GetRequired("AppHost:Minio:PresignedUrlExpiryMinutes"),
            KeycloakImage = builder.Configuration.GetRequired("AppHost:Keycloak:Image"),
            KeycloakPort = builder.Configuration.GetRequiredInt("AppHost:Keycloak:Port"),
            KeycloakHostname = builder.Configuration.GetRequired("AppHost:Keycloak:Hostname"),
            KeycloakDataVolume = builder.Configuration.GetRequired("AppHost:Keycloak:DataVolume"),
            SeqImage = builder.Configuration.GetRequired("AppHost:Seq:Image"),
            SeqPort = builder.Configuration.GetRequiredInt("AppHost:Seq:Port"),
            SeqDataVolume = builder.Configuration.GetRequired("AppHost:Seq:DataVolume"),
            SeqSinkName = builder.Configuration.GetRequired("AppHost:Seq:SinkName"),
            SeqServerUrl = builder.Configuration.GetRequired("AppHost:Seq:ServerUrl"),
            RedisImage = builder.Configuration.GetRequired("AppHost:Redis:Image"),
            RedisPort = builder.Configuration.GetRequiredInt("AppHost:Redis:Port"),
            RedisDataVolume = builder.Configuration.GetRequired("AppHost:Redis:DataVolume"),
            RedisConnectionString = builder.Configuration.GetRequired("AppHost:Redis:ConnectionString"),
            MailpitImage = builder.Configuration.GetRequired("AppHost:Mailpit:Image"),
            MailpitSmtpPort = builder.Configuration.GetRequiredInt("AppHost:Mailpit:SmtpPort"),
            MailpitUiPort = builder.Configuration.GetRequiredInt("AppHost:Mailpit:UiPort"),
            WebAppCommand = builder.Configuration.GetRequired("AppHost:WebApp:Command"),
            WebAppSourcePath = Path.GetFullPath(builder.Configuration.GetRequired("AppHost:WebApp:SourcePath"), builder.AppHostDirectory),
            WebAppPort = builder.Configuration.GetRequiredInt("AppHost:WebApp:Port"),
            StripeCliCommand = builder.Configuration.GetRequired("AppHost:StripeCli:Command"),
            StripeCliForwardTo = builder.Configuration.GetRequired("AppHost:StripeCli:ForwardTo"),
            StripeCliEvents = builder.Configuration.GetRequired("AppHost:StripeCli:Events"),
            AuthenticationAudience = builder.Configuration.GetRequired("AppHost:Authentication:Audience"),
            AuthenticationMetadataUrl = builder.Configuration.GetRequired("AppHost:Authentication:MetadataUrl"),
            AuthenticationRequireHttpsMetadata = builder.Configuration.GetRequired("AppHost:Authentication:RequireHttpsMetadata"),
            AuthenticationIssuer = builder.Configuration.GetRequired("AppHost:Authentication:Issuer"),
            KeycloakAdminUrl = builder.Configuration.GetRequired("AppHost:Authentication:Keycloak:AdminUrl"),
            KeycloakTokenUrl = builder.Configuration.GetRequired("AppHost:Authentication:Keycloak:TokenUrl"),
            KeycloakAdminClientId = builder.Configuration.GetRequired("AppHost:Authentication:Keycloak:AdminClientId"),
            KeycloakAuthClientId = builder.Configuration.GetRequired("AppHost:Authentication:Keycloak:AuthClientId"),
            BootstrapAdminEnabled = builder.Configuration.GetRequired("AppHost:Authentication:BootstrapAdmin:Enabled"),
            BootstrapAdminEmail = builder.Configuration.GetRequired("AppHost:Authentication:BootstrapAdmin:Email"),
            BootstrapAdminFirstName = builder.Configuration.GetRequired("AppHost:Authentication:BootstrapAdmin:FirstName"),
            BootstrapAdminLastName = builder.Configuration.GetRequired("AppHost:Authentication:BootstrapAdmin:LastName"),
            NotificationEmailConfirmationUrlTemplate = builder.Configuration.GetRequired("AppHost:Notifications:EmailConfirmationUrlTemplate"),
            NotificationSmtpHost = builder.Configuration.GetRequired("AppHost:Notifications:Smtp:Host"),
            NotificationSmtpPort = builder.Configuration.GetRequired("AppHost:Notifications:Smtp:Port"),
            NotificationSmtpEnableSsl = builder.Configuration.GetRequired("AppHost:Notifications:Smtp:EnableSsl"),
            NotificationSmtpFromName = builder.Configuration.GetRequired("AppHost:Notifications:Smtp:FromName"),
            NotificationSmtpTimeoutSeconds = builder.Configuration.GetRequired("AppHost:Notifications:Smtp:TimeoutSeconds"),
            ProjectEnvironment = builder.Configuration.GetRequired("AppHost:Projects:Environment"),
            AuthenticationApiPort = builder.Configuration.GetRequiredInt("AppHost:Projects:AuthenticationApi:HttpPort"),
            AuthenticationApiHttpsPort = builder.Configuration.GetRequiredInt("AppHost:Projects:AuthenticationApi:HttpsPort"),
            ProductApiPort = builder.Configuration.GetRequiredInt("AppHost:Projects:ProductApi:HttpPort"),
            ProductApiHttpsPort = builder.Configuration.GetRequiredInt("AppHost:Projects:ProductApi:HttpsPort"),
            OrderApiPort = builder.Configuration.GetRequiredInt("AppHost:Projects:OrderApi:HttpPort"),
            OrderApiHttpsPort = builder.Configuration.GetRequiredInt("AppHost:Projects:OrderApi:HttpsPort"),
            PaymentApiPort = builder.Configuration.GetRequiredInt("AppHost:Projects:PaymentApi:HttpPort"),
            PaymentApiHttpsPort = builder.Configuration.GetRequiredInt("AppHost:Projects:PaymentApi:HttpsPort"),
            UserApiPort = builder.Configuration.GetRequiredInt("AppHost:Projects:UserApi:HttpPort"),
            UserApiHttpsPort = builder.Configuration.GetRequiredInt("AppHost:Projects:UserApi:HttpsPort"),
            SellerApiPort = builder.Configuration.GetRequiredInt("AppHost:Projects:SellerApi:HttpPort"),
            SellerApiHttpsPort = builder.Configuration.GetRequiredInt("AppHost:Projects:SellerApi:HttpsPort"),
            ImageApiPort = builder.Configuration.GetRequiredInt("AppHost:Projects:ImageApi:HttpPort"),
            ImageApiHttpsPort = builder.Configuration.GetRequiredInt("AppHost:Projects:ImageApi:HttpsPort"),
            NotificationApiPort = builder.Configuration.GetRequiredInt("AppHost:Projects:NotificationApi:HttpPort"),
            NotificationApiHttpsPort = builder.Configuration.GetRequiredInt("AppHost:Projects:NotificationApi:HttpsPort"),
            MessagingApiPort = builder.Configuration.GetRequiredInt("AppHost:Projects:MessagingApi:HttpPort"),
            MessagingApiHttpsPort = builder.Configuration.GetRequiredInt("AppHost:Projects:MessagingApi:HttpsPort"),
            GatewayApiPort = builder.Configuration.GetRequiredInt("AppHost:Projects:GatewayApi:HttpPort"),
            GatewayApiHttpsPort = builder.Configuration.GetRequiredInt("AppHost:Projects:GatewayApi:HttpsPort")
        };
    }
}
