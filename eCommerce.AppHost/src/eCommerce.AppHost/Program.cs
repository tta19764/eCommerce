var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume("ecommerce-postgres-data");

var productDb = postgres.AddDatabase("product-db", "product_db");
var orderDb = postgres.AddDatabase("order-db", "order_db");
var userDb = postgres.AddDatabase("user-db", "user_db");
var imageDb = postgres.AddDatabase("image-db", "image_db");

var rabbitMq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin()
    .WithDataVolume("ecommerce-rabbitmq-data");

var minio = builder.AddContainer("minio", "minio/minio")
    .WithArgs("server", "/data", "--console-address", ":9001")
    .WithEnvironment("MINIO_ROOT_USER", "minioadmin")
    .WithEnvironment("MINIO_ROOT_PASSWORD", "minioadmin")
    .WithHttpEndpoint(port: 9000, targetPort: 9000, name: "api")
    .WithHttpEndpoint(port: 9001, targetPort: 9001, name: "console")
    .WithVolume("ecommerce-minio-data", "/data");

var authenticationApi = builder.AddProject<Projects.AuthenticationApi_Api>("authentication-api");

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
