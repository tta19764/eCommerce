using GatewayApi.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddGatewaySwagger(builder.Configuration);
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapGatewaySwaggerDocuments();
    app.UseGatewaySwaggerUi();
}

app.UseHttpsRedirection();

app.UseGatewaySignature();

app.MapReverseProxy();

app.Run();
