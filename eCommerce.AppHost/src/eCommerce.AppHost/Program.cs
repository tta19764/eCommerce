using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

// Aspire parameters are the bridge between local appsettings values, user secrets, environment
// variables, and container/project environment variables. Values marked as secret are redacted
// by Aspire and should be supplied from configuration outside source control for production.
var postgresUser = builder.AddParameter("postgres-user");
var postgresPassword = builder.AddParameter("postgres-password", secret: true);
var rabbitMqUser = builder.AddParameter("rabbitmq-user");
var rabbitMqPassword = builder.AddParameter("rabbitmq-password", secret: true);
var gatewaySignature = builder.AddParameter("gateway-signature", secret: true);
var minioRootUser = builder.AddParameter("minio-root-user");
var minioRootPassword = builder.AddParameter("minio-root-password", secret: true);
var keycloakAdminUser = builder.AddParameter("keycloak-admin-user");
var keycloakAdminPassword = builder.AddParameter("keycloak-admin-password", secret: true);
var keycloakAdminClientSecret = builder.AddParameter("keycloak-admin-client-secret", secret: true);
var keycloakAuthClientSecret = builder.AddParameter("keycloak-auth-client-secret", secret: true);

// Infrastructure configuration is read once at startup and then passed into the Aspire resources.
// Keeping these values in configuration avoids hard-coded container image tags, host ports, and
// volume names in this file. Production can override the same keys through environment variables
// or deployment configuration without changing AppHost code.
var postgresPort = GetRequiredInt("AppHost:Postgres:Port");
var postgresImageTag = GetRequired("AppHost:Postgres:ImageTag");
var postgresDataVolume = GetRequired("AppHost:Postgres:DataVolume");

// Each service owns its own database. Aspire creates the databases on the shared PostgreSQL
// container and injects the selected database connection string into the matching project through
// WithReference(..., "Database").
var productDbName = GetRequired("AppHost:Postgres:Databases:Product");
var orderDbName = GetRequired("AppHost:Postgres:Databases:Order");
var userDbName = GetRequired("AppHost:Postgres:Databases:User");
var imageDbName = GetRequired("AppHost:Postgres:Databases:Image");
var authenticationDbName = GetRequired("AppHost:Postgres:Databases:Authentication");
var notificationDbName = GetRequired("AppHost:Postgres:Databases:Notification");

var rabbitMqPort = GetRequiredInt("AppHost:RabbitMq:Port");
var rabbitMqDataVolume = GetRequired("AppHost:RabbitMq:DataVolume");

// Resource APIs use GatewayOnlyMiddleware to reject direct browser or client traffic. The gateway
// adds this shared header/signature pair when proxying requests, and AppHost injects the same
// values into every API so validation is consistent across local runs.
var gatewayHeaderName = GetRequired("AppHost:Gateway:HeaderName");

// MinIO is the local object-storage dependency used by ImageApi. It exposes both the S3-compatible
// API endpoint and the management console, with data persisted in a named Docker volume.
var minioImage = GetRequired("AppHost:Minio:Image");
var minioApiPort = GetRequiredInt("AppHost:Minio:ApiPort");
var minioConsolePort = GetRequiredInt("AppHost:Minio:ConsolePort");
var minioDataVolume = GetRequired("AppHost:Minio:DataVolume");

// Keycloak is run as an actual local identity provider rather than mocked auth. AuthenticationApi
// uses the admin client to create/delete users and the auth client for login/token flows.
var keycloakImage = GetRequired("AppHost:Keycloak:Image");
var keycloakPort = GetRequiredInt("AppHost:Keycloak:Port");
var keycloakHostname = GetRequired("AppHost:Keycloak:Hostname");
var keycloakDataVolume = GetRequired("AppHost:Keycloak:DataVolume");

// Seq is the central local log sink. Service appsettings define Serilog sink slots, and AppHost
// overrides the Seq serverUrl slot so all APIs write to the Aspire-managed Seq container.
var seqImage = GetRequired("AppHost:Seq:Image");
var seqPort = GetRequiredInt("AppHost:Seq:Port");
var seqDataVolume = GetRequired("AppHost:Seq:DataVolume");
var seqServerUrl = GetRequired("AppHost:Seq:ServerUrl");

// Redis backs query caching for read-heavy endpoints. The services still fall back to in-memory
// caching when no Redis connection string is provided, but AppHost supplies a shared container so
// cached pages are consistent across the local microservice processes.
var redisImage = GetRequired("AppHost:Redis:Image");
var redisPort = GetRequiredInt("AppHost:Redis:Port");
var redisDataVolume = GetRequired("AppHost:Redis:DataVolume");
var redisConnectionString = GetRequired("AppHost:Redis:ConnectionString");

// Mailpit is the local SMTP target for NotificationApi. It accepts SMTP on one endpoint and exposes
// a browser inbox on another, which lets development exercise real SMTP delivery without external
// credentials or sending messages outside the developer machine.
var mailpitImage = GetRequired("AppHost:Mailpit:Image");
var mailpitSmtpPort = GetRequiredInt("AppHost:Mailpit:SmtpPort");
var mailpitUiPort = GetRequiredInt("AppHost:Mailpit:UiPort");

// Authentication settings are injected into AuthenticationApi so local Keycloak URLs, issuer,
// audience, and HTTPS metadata behavior are controlled from AppHost configuration.
var authenticationAudience = GetRequired("AppHost:Authentication:Audience");
var authenticationMetadataUrl = GetRequired("AppHost:Authentication:MetadataUrl");
var authenticationRequireHttpsMetadata = GetRequired("AppHost:Authentication:RequireHttpsMetadata");
var authenticationIssuer = GetRequired("AppHost:Authentication:Issuer");

// Keycloak client identifiers are non-secret configuration; client secrets stay as Aspire secret
// parameters above. This split lets production use secure configuration providers for secrets while
// still making local development explicit and reproducible.
var keycloakAdminUrl = GetRequired("AppHost:Authentication:Keycloak:AdminUrl");
var keycloakTokenUrl = GetRequired("AppHost:Authentication:Keycloak:TokenUrl");
var keycloakAdminClientId = GetRequired("AppHost:Authentication:Keycloak:AdminClientId");
var keycloakAuthClientId = GetRequired("AppHost:Authentication:Keycloak:AuthClientId");

// Notification settings are passed to NotificationApi so background email content and SMTP delivery
// stay environment-specific. Production should override the same keys with a real SMTP host and
// credentials from a secure configuration source.
var notificationFromAddress = GetRequired("AppHost:Notifications:FromAddress");
var notificationEmailConfirmationUrlTemplate = GetRequired("AppHost:Notifications:EmailConfirmationUrlTemplate");
var notificationSmtpHost = GetRequired("AppHost:Notifications:Smtp:Host");
var notificationSmtpPort = GetRequired("AppHost:Notifications:Smtp:Port");
var notificationSmtpEnableSsl = GetRequired("AppHost:Notifications:Smtp:EnableSsl");
var notificationSmtpFromName = GetRequired("AppHost:Notifications:Smtp:FromName");
var notificationSmtpTimeoutSeconds = GetRequired("AppHost:Notifications:Smtp:TimeoutSeconds");

// Project ports are pinned so the gateway, Swagger UI, and external tools can use stable localhost
// URLs. Aspire still wires service references internally, but fixed ports make manual testing and
// Keycloak redirect/client configuration predictable.
var authenticationApiPort = GetRequiredInt("AppHost:Projects:AuthenticationApi:HttpPort");
var authenticationApiHttpsPort = GetRequiredInt("AppHost:Projects:AuthenticationApi:HttpsPort");
var productApiPort = GetRequiredInt("AppHost:Projects:ProductApi:HttpPort");
var productApiHttpsPort = GetRequiredInt("AppHost:Projects:ProductApi:HttpsPort");
var orderApiPort = GetRequiredInt("AppHost:Projects:OrderApi:HttpPort");
var orderApiHttpsPort = GetRequiredInt("AppHost:Projects:OrderApi:HttpsPort");
var userApiPort = GetRequiredInt("AppHost:Projects:UserApi:HttpPort");
var userApiHttpsPort = GetRequiredInt("AppHost:Projects:UserApi:HttpsPort");
var imageApiPort = GetRequiredInt("AppHost:Projects:ImageApi:HttpPort");
var imageApiHttpsPort = GetRequiredInt("AppHost:Projects:ImageApi:HttpsPort");
var notificationApiPort = GetRequiredInt("AppHost:Projects:NotificationApi:HttpPort");
var notificationApiHttpsPort = GetRequiredInt("AppHost:Projects:NotificationApi:HttpsPort");
var gatewayApiPort = GetRequiredInt("AppHost:Projects:GatewayApi:HttpPort");
var gatewayApiHttpsPort = GetRequiredInt("AppHost:Projects:GatewayApi:HttpsPort");

// PostgreSQL is shared infrastructure, but each microservice gets a separate logical database.
// The 18.x image stores cluster data under version-specific subdirectories, so the named volume
// should be treated as tied to this major version unless a pg_upgrade flow is performed.
var postgres = builder.AddPostgres("postgres", postgresUser, postgresPassword, port: postgresPort)
    .WithImageTag(postgresImageTag)
    .WithDataVolume(postgresDataVolume);

var productDb = postgres.AddDatabase("product-db", productDbName);
var orderDb = postgres.AddDatabase("order-db", orderDbName);
var userDb = postgres.AddDatabase("user-db", userDbName);
var imageDb = postgres.AddDatabase("image-db", imageDbName);
var authenticationDb = postgres.AddDatabase("authentication-db", authenticationDbName);
var notificationDb = postgres.AddDatabase("notification-db", notificationDbName);

// RabbitMQ backs service-to-service request/response messaging through MassTransit. Explicit local
// credentials prevent accidental default guest mismatches and keep connection strings generated by
// Aspire aligned with the service appsettings.
var rabbitMq = builder.AddRabbitMQ("rabbitmq", rabbitMqUser, rabbitMqPassword, rabbitMqPort)
    .WithManagementPlugin()
    .WithDataVolume(rabbitMqDataVolume);

// MinIO is configured with two endpoints: "api" for S3-compatible object operations and "console"
// for browser administration. ImageApi waits for this resource before starting.
var minio = builder.AddContainer("minio", minioImage)
    .WithArgs("server", "/data", "--console-address", ":9001")
    .WithEnvironment("MINIO_ROOT_USER", minioRootUser)
    .WithEnvironment("MINIO_ROOT_PASSWORD", minioRootPassword)
    .WithHttpEndpoint(port: minioApiPort, targetPort: 9000, name: "api")
    .WithHttpEndpoint(port: minioConsolePort, targetPort: 9001, name: "console")
    .WithVolume(minioDataVolume, "/data");

// Keycloak uses start-dev for local configuration work. The hostname is set to the same public URL
// that services and browsers use so issuer and discovery URLs remain stable during development.
var keycloak = builder.AddContainer("keycloak", keycloakImage)
    .WithArgs("start-dev", $"--http-port={keycloakPort}", $"--hostname={keycloakHostname}")
    .WithEnvironment("KEYCLOAK_ADMIN", keycloakAdminUser)
    .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", keycloakAdminPassword)
    .WithHttpEndpoint(port: keycloakPort, targetPort: keycloakPort, name: "http")
    .WithVolume(keycloakDataVolume, "/opt/keycloak/data");

// Seq requires accepting its EULA. Local authentication is disabled here to keep development logs
// easy to inspect; production should provide an admin password or equivalent secure setup.
var seq = builder.AddContainer("seq", seqImage)
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithEnvironment("SEQ_FIRSTRUN_NOAUTHENTICATION", "true")
    .WithHttpEndpoint(port: seqPort, targetPort: 80, name: "http")
    .WithVolume(seqDataVolume, "/data");

// Redis is intentionally lightweight local infrastructure: append-only persistence keeps cache data
// across restarts for easier inspection, while short application TTLs keep stale read models bounded.
var redis = builder.AddContainer("redis", redisImage)
    .WithArgs("redis-server", "--appendonly", "yes")
    .WithEndpoint(port: redisPort, targetPort: 6379, name: "tcp")
    .WithVolume(redisDataVolume, "/data");

// Mailpit receives development SMTP messages and keeps them in an inspectable local inbox.
var mailpit = builder.AddContainer("mailpit", mailpitImage)
    .WithEndpoint(port: mailpitSmtpPort, targetPort: 1025, name: "smtp")
    .WithHttpEndpoint(port: mailpitUiPort, targetPort: 8025, name: "http");

// AuthenticationApi owns identity/account workflows. It depends on PostgreSQL for local account
// state, RabbitMQ for profile creation requests to UserApi, Keycloak for real identity management,
// and Seq for centralized logs.
var authenticationApi = builder.AddProject<Projects.AuthenticationApi_Api>("authentication-api")
    .WithHttpEndpoint(port: authenticationApiPort)
    .WithHttpsEndpoint(port: authenticationApiHttpsPort)
    .WithEnvironment("Gateway__HeaderName", gatewayHeaderName)
    .WithEnvironment("Gateway__Signature", gatewaySignature)
    .WithEnvironment("Serilog__WriteTo__1__Args__serverUrl", seqServerUrl)
    .WithEnvironment("ConnectionStrings__Redis", redisConnectionString)
    .WithReference(authenticationDb, "Database")
    .WithReference(rabbitMq)
    .WithEnvironment("Authentication__Audience", authenticationAudience)
    .WithEnvironment("Authentication__MetadataUrl", authenticationMetadataUrl)
    .WithEnvironment("Authentication__RequireHttpsMetadata", authenticationRequireHttpsMetadata)
    .WithEnvironment("Authentication__Issuer", authenticationIssuer)
    .WithEnvironment("Keycloak__AdminUrl", keycloakAdminUrl)
    .WithEnvironment("Keycloak__TokenUrl", keycloakTokenUrl)
    .WithEnvironment("Keycloak__AdminClientId", keycloakAdminClientId)
    .WithEnvironment("Keycloak__AdminClientSecret", keycloakAdminClientSecret)
    .WithEnvironment("Keycloak__AuthClientId", keycloakAuthClientId)
    .WithEnvironment("Keycloak__AuthClientSecret", keycloakAuthClientSecret)
    .WaitFor(postgres)
    .WaitFor(rabbitMq)
    .WaitFor(keycloak)
    .WaitFor(redis)
    .WaitFor(seq);

// ProductApi owns catalog data and publishes/serves product-related messages through RabbitMQ.
// Gateway settings are injected even when testing locally so direct service access remains blocked
// in the same way it is expected to be blocked behind the gateway.
var productApi = builder.AddProject<Projects.ProductApi_Api>("product-api")
    .WithHttpEndpoint(port: productApiPort)
    .WithHttpsEndpoint(port: productApiHttpsPort)
    .WithEnvironment("Gateway__HeaderName", gatewayHeaderName)
    .WithEnvironment("Gateway__Signature", gatewaySignature)
    .WithEnvironment("Serilog__WriteTo__1__Args__serverUrl", seqServerUrl)
    .WithEnvironment("ConnectionStrings__Redis", redisConnectionString)
    .WithReference(productDb, "Database")
    .WithReference(rabbitMq)
    .WaitFor(postgres)
    .WaitFor(rabbitMq)
    .WaitFor(redis)
    .WaitFor(seq);

// OrderApi owns order state. It depends on RabbitMQ because order workflows call other services for
// product/user data, and it gets its own PostgreSQL database so order schema changes stay isolated.
var orderApi = builder.AddProject<Projects.OrderApi_Api>("order-api")
    .WithHttpEndpoint(port: orderApiPort)
    .WithHttpsEndpoint(port: orderApiHttpsPort)
    .WithEnvironment("Gateway__HeaderName", gatewayHeaderName)
    .WithEnvironment("Gateway__Signature", gatewaySignature)
    .WithEnvironment("Serilog__WriteTo__1__Args__serverUrl", seqServerUrl)
    .WithEnvironment("ConnectionStrings__Redis", redisConnectionString)
    .WithReference(orderDb, "Database")
    .WithReference(rabbitMq)
    .WaitFor(postgres)
    .WaitFor(rabbitMq)
    .WaitFor(redis)
    .WaitFor(seq);

// UserApi owns profile data linked from AuthenticationApi accounts. AuthenticationApi creates and
// deletes profiles through MassTransit, so UserApi must wait for RabbitMQ and PostgreSQL before it
// can reliably process profile messages.
var userApi = builder.AddProject<Projects.UserApi_Api>("user-api")
    .WithHttpEndpoint(port: userApiPort)
    .WithHttpsEndpoint(port: userApiHttpsPort)
    .WithEnvironment("Gateway__HeaderName", gatewayHeaderName)
    .WithEnvironment("Gateway__Signature", gatewaySignature)
    .WithEnvironment("Serilog__WriteTo__1__Args__serverUrl", seqServerUrl)
    .WithReference(userDb, "Database")
    .WithReference(rabbitMq)
    .WaitFor(postgres)
    .WaitFor(rabbitMq)
    .WaitFor(seq);

// ImageApi owns image metadata in PostgreSQL and binary object storage in MinIO. It also listens on
// RabbitMQ for image-reference validation requests from services that store ImageId values.
var imageApi = builder.AddProject<Projects.ImageApi_Api>("image-api")
    .WithHttpEndpoint(port: imageApiPort)
    .WithHttpsEndpoint(port: imageApiHttpsPort)
    .WithEnvironment("Gateway__HeaderName", gatewayHeaderName)
    .WithEnvironment("Gateway__Signature", gatewaySignature)
    .WithEnvironment("Serilog__WriteTo__1__Args__serverUrl", seqServerUrl)
    .WithReference(imageDb, "Database")
    .WithReference(rabbitMq)
    .WaitFor(minio)
    .WaitFor(postgres)
    .WaitFor(rabbitMq)
    .WaitFor(seq);

// NotificationApi owns durable notification jobs. Other services publish notification requests to
// RabbitMQ, this service stores them in its PostgreSQL database, and its hosted worker retries
// delivery independently of the user-facing request that created the job.
var notificationApi = builder.AddProject<Projects.NotificationApi_Api>("notification-api")
    .WithHttpEndpoint(port: notificationApiPort)
    .WithHttpsEndpoint(port: notificationApiHttpsPort)
    .WithEnvironment("Serilog__WriteTo__1__Args__serverUrl", seqServerUrl)
    .WithEnvironment("Email__FromAddress", notificationFromAddress)
    .WithEnvironment("Email__EmailConfirmationUrlTemplate", notificationEmailConfirmationUrlTemplate)
    .WithEnvironment("Smtp__Host", notificationSmtpHost)
    .WithEnvironment("Smtp__Port", notificationSmtpPort)
    .WithEnvironment("Smtp__EnableSsl", notificationSmtpEnableSsl)
    .WithEnvironment("Smtp__FromName", notificationSmtpFromName)
    .WithEnvironment("Smtp__TimeoutSeconds", notificationSmtpTimeoutSeconds)
    .WithReference(notificationDb, "Database")
    .WithReference(rabbitMq)
    .WaitFor(postgres)
    .WaitFor(rabbitMq)
    .WaitFor(mailpit)
    .WaitFor(seq);

// GatewayApi is the only externally exposed API entry point. It proxies to the backend service
// HTTPS endpoints, adds the gateway signature on forwarded requests, and serves the combined
// Swagger UI for the downstream APIs.
builder.AddProject<Projects.GatewayApi_Api>("gateway-api")
    .WithHttpEndpoint(port: gatewayApiPort)
    .WithHttpsEndpoint(port: gatewayApiHttpsPort)
    .WithEnvironment("Gateway__HeaderName", gatewayHeaderName)
    .WithEnvironment("Gateway__Signature", gatewaySignature)
    .WithReference(authenticationApi)
    .WithReference(productApi)
    .WithReference(orderApi)
    .WithReference(userApi)
    .WithReference(imageApi)
    .WithReference(notificationApi)
    .WaitFor(authenticationApi)
    .WaitFor(productApi)
    .WaitFor(orderApi)
    .WaitFor(userApi)
    .WaitFor(imageApi)
    .WaitFor(notificationApi)
    .WithExternalHttpEndpoints();

await builder.Build().RunAsync();

// Fail fast when required configuration is missing. A missing AppHost value usually means a service
// would start with an invalid connection string, wrong public URL, or disabled security boundary, so
// surfacing the exact key at startup is preferable to failing later inside a container or API.
string GetRequired(string key) =>
    builder.Configuration[key] ??
    throw new InvalidOperationException($"Missing required configuration value '{key}'.");

int GetRequiredInt(string key) =>
    builder.Configuration.GetValue<int?>(key) ??
    throw new InvalidOperationException($"Missing required configuration value '{key}'.");
