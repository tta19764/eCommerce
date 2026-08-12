using eCommerce.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddEcommerceResources();

await builder.Build().RunAsync();
