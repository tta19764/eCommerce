using GatewayApi.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddGatewaySwagger(builder.Configuration);
builder.Services.AddGatewayCors();
builder.Services.AddGatewayReverseProxy(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapGatewaySwaggerDocuments();
    app.UseGatewaySwaggerUi();
}

app.UseHttpsRedirection();
app.UseGatewayCors();

app.MapReverseProxy();

app.Run();
