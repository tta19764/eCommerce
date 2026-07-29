using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

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

var postgresPort = GetRequiredInt("AppHost:Postgres:Port");
var postgresImageTag = GetRequired("AppHost:Postgres:ImageTag");
var postgresDataVolume = GetRequired("AppHost:Postgres:DataVolume");

var productDbName = GetRequired("AppHost:Postgres:Databases:Product");
var orderDbName = GetRequired("AppHost:Postgres:Databases:Order");
var userDbName = GetRequired("AppHost:Postgres:Databases:User");
var imageDbName = GetRequired("AppHost:Postgres:Databases:Image");
var authenticationDbName = GetRequired("AppHost:Postgres:Databases:Authentication");

var rabbitMqPort = GetRequiredInt("AppHost:RabbitMq:Port");
var rabbitMqDataVolume = GetRequired("AppHost:RabbitMq:DataVolume");

var gatewayHeaderName = GetRequired("AppHost:Gateway:HeaderName");

var minioImage = GetRequired("AppHost:Minio:Image");
var minioApiPort = GetRequiredInt("AppHost:Minio:ApiPort");
var minioConsolePort = GetRequiredInt("AppHost:Minio:ConsolePort");
var minioDataVolume = GetRequired("AppHost:Minio:DataVolume");

var keycloakImage = GetRequired("AppHost:Keycloak:Image");
var keycloakPort = GetRequiredInt("AppHost:Keycloak:Port");
var keycloakHostname = GetRequired("AppHost:Keycloak:Hostname");
var keycloakDataVolume = GetRequired("AppHost:Keycloak:DataVolume");

var seqImage = GetRequired("AppHost:Seq:Image");
var seqPort = GetRequiredInt("AppHost:Seq:Port");
var seqDataVolume = GetRequired("AppHost:Seq:DataVolume");
var seqServerUrl = GetRequired("AppHost:Seq:ServerUrl");

var authenticationAudience = GetRequired("AppHost:Authentication:Audience");
var authenticationMetadataUrl = GetRequired("AppHost:Authentication:MetadataUrl");
var authenticationRequireHttpsMetadata = GetRequired("AppHost:Authentication:RequireHttpsMetadata");
var authenticationIssuer = GetRequired("AppHost:Authentication:Issuer");

var keycloakAdminUrl = GetRequired("AppHost:Authentication:Keycloak:AdminUrl");
var keycloakTokenUrl = GetRequired("AppHost:Authentication:Keycloak:TokenUrl");
var keycloakAdminClientId = GetRequired("AppHost:Authentication:Keycloak:AdminClientId");
var keycloakAuthClientId = GetRequired("AppHost:Authentication:Keycloak:AuthClientId");

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
var gatewayApiPort = GetRequiredInt("AppHost:Projects:GatewayApi:HttpPort");
var gatewayApiHttpsPort = GetRequiredInt("AppHost:Projects:GatewayApi:HttpsPort");

var postgres = builder.AddPostgres("postgres", postgresUser, postgresPassword, port: postgresPort)
    .WithImageTag(postgresImageTag)
    .WithDataVolume(postgresDataVolume);

var productDb = postgres.AddDatabase("product-db", productDbName);
var orderDb = postgres.AddDatabase("order-db", orderDbName);
var userDb = postgres.AddDatabase("user-db", userDbName);
var imageDb = postgres.AddDatabase("image-db", imageDbName);
var authenticationDb = postgres.AddDatabase("authentication-db", authenticationDbName);

var rabbitMq = builder.AddRabbitMQ("rabbitmq", rabbitMqUser, rabbitMqPassword, rabbitMqPort)
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

var seq = builder.AddContainer("seq", seqImage)
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithEnvironment("SEQ_FIRSTRUN_NOAUTHENTICATION", "true")
    .WithHttpEndpoint(port: seqPort, targetPort: 80, name: "http")
    .WithVolume(seqDataVolume, "/data");

var authenticationApi = builder.AddProject<Projects.AuthenticationApi_Api>("authentication-api")
    .WithHttpEndpoint(port: authenticationApiPort)
    .WithHttpsEndpoint(port: authenticationApiHttpsPort)
    .WithEnvironment("Gateway__HeaderName", gatewayHeaderName)
    .WithEnvironment("Gateway__Signature", gatewaySignature)
    .WithEnvironment("Serilog__WriteTo__1__Args__serverUrl", seqServerUrl)
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
    .WaitFor(seq);

var productApi = builder.AddProject<Projects.ProductApi_Api>("product-api")
    .WithHttpEndpoint(port: productApiPort)
    .WithHttpsEndpoint(port: productApiHttpsPort)
    .WithEnvironment("Gateway__HeaderName", gatewayHeaderName)
    .WithEnvironment("Gateway__Signature", gatewaySignature)
    .WithEnvironment("Serilog__WriteTo__1__Args__serverUrl", seqServerUrl)
    .WithReference(productDb, "Database")
    .WithReference(rabbitMq)
    .WaitFor(postgres)
    .WaitFor(rabbitMq)
    .WaitFor(seq);

var orderApi = builder.AddProject<Projects.OrderApi_Api>("order-api")
    .WithHttpEndpoint(port: orderApiPort)
    .WithHttpsEndpoint(port: orderApiHttpsPort)
    .WithEnvironment("Gateway__HeaderName", gatewayHeaderName)
    .WithEnvironment("Gateway__Signature", gatewaySignature)
    .WithEnvironment("Serilog__WriteTo__1__Args__serverUrl", seqServerUrl)
    .WithReference(orderDb, "Database")
    .WithReference(rabbitMq)
    .WaitFor(postgres)
    .WaitFor(rabbitMq)
    .WaitFor(seq);

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

var imageApi = builder.AddProject<Projects.ImageApi_Api>("image-api")
    .WithHttpEndpoint(port: imageApiPort)
    .WithHttpsEndpoint(port: imageApiHttpsPort)
    .WithEnvironment("Gateway__HeaderName", gatewayHeaderName)
    .WithEnvironment("Gateway__Signature", gatewaySignature)
    .WithEnvironment("Serilog__WriteTo__1__Args__serverUrl", seqServerUrl)
    .WithReference(imageDb, "Database")
    .WaitFor(minio)
    .WaitFor(postgres)
    .WaitFor(seq);

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
