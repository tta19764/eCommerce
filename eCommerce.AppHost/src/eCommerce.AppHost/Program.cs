var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume("ecommerce-postgres-data");

var productDb = postgres.AddDatabase("product-db", "product_db");
var orderDb = postgres.AddDatabase("order-db", "order_db");

var rabbitMq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin()
    .WithDataVolume("ecommerce-rabbitmq-data");

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

builder.AddProject<Projects.GatewayApi_Api>("gateway-api")
    .WithReference(authenticationApi)
    .WithReference(productApi)
    .WithReference(orderApi)
    .WaitFor(authenticationApi)
    .WaitFor(productApi)
    .WaitFor(orderApi)
    .WithExternalHttpEndpoints();

await builder.Build().RunAsync();
