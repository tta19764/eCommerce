using AuthenticationApi.Application;
using AuthenticationApi.Api.Extensions;
using AuthenticationApi.Infrastructure;
using SharedLibrary.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSharedSerilog();

builder.Services.AddApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await AuthenticationDataSeeder.SeedAsync(app.Services);
}

app.UseHttpsRedirection();

app.UseSharedMiddleware();

app.MapEndpoints();

app.Run();
