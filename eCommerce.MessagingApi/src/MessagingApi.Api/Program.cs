using MessagingApi.Api.Extensions;
using MessagingApi.Application;
using MessagingApi.Infrastructure;
using SharedLibrary.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSharedSerilog();

builder.Services.AddApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.ApplyMigrations<MessagingDbContext>();
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseSharedMiddleware();

app.MapEndpoints();

app.Run();

