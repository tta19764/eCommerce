using ProductApi.Api.Extensions;
using ProductApi.Application;
using ProductApi.Infrastructure;
using SharedLibrary.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSharedSerilog();

builder.Services.AddApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.ApplyMigrations<ProductDbContext>();
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseSharedMiddleware();

app.MapEndpoints();

app.Run();

/// <summary>
/// Entry point marker used by integration tests.
/// </summary>
public partial class Program;
