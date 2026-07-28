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

app.Use(async (context, next) =>
{
    var headerName = builder.Configuration["Gateway:HeaderName"];
    var signature = builder.Configuration["Gateway:Signature"];

    if (!string.IsNullOrWhiteSpace(headerName) && !string.IsNullOrWhiteSpace(signature))
    {
        context.Request.Headers[headerName] = signature;
    }

    await next();
});

app.MapReverseProxy();

app.Run();
