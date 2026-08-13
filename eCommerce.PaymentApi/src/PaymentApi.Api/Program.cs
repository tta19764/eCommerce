using PaymentApi.Api.Extensions;
using PaymentApi.Application;
using PaymentApi.Infrastructure;
using SharedLibrary.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSharedSerilog();
builder.Services.AddApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.ApplyMigrations<PaymentDbContext>();
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseSharedMiddleware();
app.MapEndpoints();
app.Run();
