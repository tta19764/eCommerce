using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var postgresUser = builder.AddParameter("postgres-user");
var postgresPassword = builder.AddParameter("postgres-password", secret: true);
var minioRootUser = builder.AddParameter("minio-root-user");
var minioRootPassword = builder.AddParameter("minio-root-password", secret: true);
var keycloakAdminUser = builder.AddParameter("keycloak-admin-user");
var keycloakAdminPassword = builder.AddParameter("keycloak-admin-password", secret: true);
var keycloakAdminClientSecret = builder.AddParameter("keycloak-admin-client-secret", secret: true);
var keycloakAuthClientSecret = builder.AddParameter("keycloak-auth-client-secret", secret: true);

var postgresPort = GetRequiredInt("AppHost:Postgres:Port");
var postgresDataVolume = GetRequired("AppHost:Postgres:DataVolume");

var productDbName = GetRequired("AppHost:Postgres:Databases:Product");
var orderDbName = GetRequired("AppHost:Postgres:Databases:Order");
var userDbName = GetRequired("AppHost:Postgres:Databases:User");
var imageDbName = GetRequired("AppHost:Postgres:Databases:Image");
var authenticationDbName = GetRequired("AppHost:Postgres:Databases:Authentication");

var rabbitMqDataVolume = GetRequired("AppHost:RabbitMq:DataVolume");

var minioImage = GetRequired("AppHost:Minio:Image");
var minioApiPort = GetRequiredInt("AppHost:Minio:ApiPort");
var minioConsolePort = GetRequiredInt("AppHost:Minio:ConsolePort");
var minioDataVolume = GetRequired("AppHost:Minio:DataVolume");

var keycloakImage = GetRequired("AppHost:Keycloak:Image");
var keycloakPort = GetRequiredInt("AppHost:Keycloak:Port");
var keycloakHostname = GetRequired("AppHost:Keycloak:Hostname");
var keycloakDataVolume = GetRequired("AppHost:Keycloak:DataVolume");

var authenticationAudience = GetRequired("AppHost:Authentication:Audience");
var authenticationMetadataUrl = GetRequired("AppHost:Authentication:MetadataUrl");
var authenticationRequireHttpsMetadata = GetRequired("AppHost:Authentication:RequireHttpsMetadata");
var authenticationIssuer = GetRequired("AppHost:Authentication:Issuer");

var keycloakAdminUrl = GetRequired("AppHost:Authentication:Keycloak:AdminUrl");
var keycloakTokenUrl = GetRequired("AppHost:Authentication:Keycloak:TokenUrl");
var keycloakAdminClientId = GetRequired("AppHost:Authentication:Keycloak:AdminClientId");
var keycloakAuthClientId = GetRequired("AppHost:Authentication:Keycloak:AuthClientId");

var postgres = builder.AddPostgres("postgres", postgresUser, postgresPassword, port: postgresPort)
    .WithDataVolume(postgresDataVolume);

var productDb = postgres.AddDatabase("product-db", productDbName);
var orderDb = postgres.AddDatabase("order-db", orderDbName);
var userDb = postgres.AddDatabase("user-db", userDbName);
var imageDb = postgres.AddDatabase("image-db", imageDbName);
var authenticationDb = postgres.AddDatabase("authentication-db", authenticationDbName);

var rabbitMq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin()
    .WithDataVolume(rabbitMqDataVolume);

var minio = builder.AddContainer("minio", minioImage)
    .WithArgs("server", "/data", "--console-address", ":9001")
    .WithEnvironment("MINIO_ROOT_USER", minioRootUser)
    .WithEnvironment("MINIO_ROOT_PASSWORD", minioRootPassword)
    .WithHttpEndpoint(port: minioApiPort, targetPort: 9000, name: "api")
    .WithHttpEndpoint(port: minioConsolePort, targetPort: 9001, name: "console")
    .WithVolume(minioDataVolume, "/data");

var keycloak = builder.AddContainer("keycloak", keycloakImage)
    .WithArgs("start-dev", $"--http-port={keycloakPort}", $"--hostname={keycloakHostname}")
    .WithEnvironment("KEYCLOAK_ADMIN", keycloakAdminUser)
    .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", keycloakAdminPassword)
    .WithHttpEndpoint(port: keycloakPort, targetPort: keycloakPort, name: "http")
    .WithVolume(keycloakDataVolume, "/opt/keycloak/data");

var authenticationApi = builder.AddProject<Projects.AuthenticationApi_Api>("authentication-api")
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
    .WaitFor(keycloak);

var productApi = builder.AddProject<Projects.ProductApi_Api>("product-api")
    .WithReference(productDb, "Database")
    .WithReference(rabbitMq)
    .WaitFor(postgres)
    .WaitFor(rabbitMq);

var orderApi = builder.AddProject<Projects.OrderApi_Api>("order-api")
    .WithReference(orderDb, "Database")
    .WithReference(rabbitMq)
    .WaitFor(postgres)
    .WaitFor(rabbitMq);

var userApi = builder.AddProject<Projects.UserApi_Api>("user-api")
    .WithReference(userDb, "Database")
    .WithReference(rabbitMq)
    .WaitFor(postgres)
    .WaitFor(rabbitMq);

var imageApi = builder.AddProject<Projects.ImageApi_Api>("image-api")
    .WithReference(imageDb, "Database")
    .WaitFor(minio)
    .WaitFor(postgres);

builder.AddProject<Projects.GatewayApi_Api>("gateway-api")
    .WithReference(authenticationApi)
    .WithReference(productApi)
    .WithReference(orderApi)
    .WithReference(userApi)
    .WithReference(imageApi)
    .WaitFor(authenticationApi)
    .WaitFor(productApi)
    .WaitFor(orderApi)
    .WaitFor(userApi)
    .WaitFor(imageApi)
    .WithExternalHttpEndpoints();

await builder.Build().RunAsync();

string GetRequired(string key) =>
    builder.Configuration[key] ??
    throw new InvalidOperationException($"Missing required configuration value '{key}'.");

int GetRequiredInt(string key) =>
    builder.Configuration.GetValue<int?>(key) ??
    throw new InvalidOperationException($"Missing required configuration value '{key}'.");
