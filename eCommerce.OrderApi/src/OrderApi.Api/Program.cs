using OrderApi.Api.Extensions;
using OrderApi.Application;
using OrderApi.Infrastructure;
using SharedLibrary.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Compose API, application, and infrastructure boundaries explicitly so pricing, messaging, and
// exchange-rate adapters remain replaceable behind their registered interfaces.
builder.Host.UseSharedSerilog();

builder.Services.AddApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.ApplyMigrations<OrderDbContext>();
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseSharedMiddleware();
// Endpoint-specific pricing limits run after shared gateway/authentication middleware has established context.
app.UseRateLimiter();

app.MapEndpoints();

app.Run();
