using NotificationApi.Application;
using NotificationApi.Infrastructure;
using SharedLibrary.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSharedSerilog();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.ApplyMigrations<NotificationDbContext>();
}

app.Run();
