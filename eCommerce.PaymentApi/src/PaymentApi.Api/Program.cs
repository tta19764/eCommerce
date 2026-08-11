using PaymentApi.Api;
using PaymentApi.Application;
using PaymentApi.Infrastructure;
using SharedLibrary.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSharedSerilog();
builder.Services.AddPaymentApi();
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
app.MapPaymentEndpoints();
app.Run();

/// <summary>Marker partial type used by in-process API test hosts.</summary>
public partial class Program;
