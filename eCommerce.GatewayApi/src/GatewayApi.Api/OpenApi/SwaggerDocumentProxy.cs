using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;

namespace GatewayApi.Api.OpenApi;

public sealed class SwaggerDocumentProxy(
    IHttpClientFactory httpClientFactory,
    IOptions<SwaggerServiceOptions> options)
{
    public async Task<IResult> GetSwaggerDocumentAsync(string serviceName, CancellationToken cancellationToken)
    {
        var service = options.Value.Services.FirstOrDefault(
            configuredService => string.Equals(configuredService.Name, serviceName, StringComparison.OrdinalIgnoreCase));

        if (service is null)
        {
            return Results.NotFound();
        }

        try
        {
            var documentUrl = $"{service.Address.TrimEnd('/')}/{service.DocumentPath.TrimStart('/')}";
            var httpClient = httpClientFactory.CreateClient();
            using var response = await httpClient.GetAsync(documentUrl, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return Results.Problem(
                    $"OpenAPI document from {service.DisplayName} returned {(int)response.StatusCode}.",
                    statusCode: StatusCodes.Status502BadGateway);
            }

            var document = await response.Content.ReadAsStringAsync(cancellationToken);
            var json = JsonNode.Parse(document)?.AsObject();

            if (json is null)
            {
                return Results.Problem(
                    $"OpenAPI document from {service.DisplayName} could not be parsed.",
                    statusCode: StatusCodes.Status502BadGateway);
            }

            RewriteInfo(json, service);
            RewriteServers(json);
            RewritePaths(json, service.RoutePrefix);

            return Results.Json(json);
        }
        catch (HttpRequestException)
        {
            return Results.Problem(
                $"OpenAPI document from {service.DisplayName} could not be reached.",
                statusCode: StatusCodes.Status502BadGateway);
        }
        catch (JsonException)
        {
            return Results.Problem(
                $"OpenAPI document from {service.DisplayName} could not be parsed.",
                statusCode: StatusCodes.Status502BadGateway);
        }
    }

    private static void RewriteInfo(JsonObject document, SwaggerServiceDescriptor service)
    {
        var info = document["info"] as JsonObject ?? [];
        info["title"] = service.DisplayName;
        document["info"] = info;
    }

    private static void RewriteServers(JsonObject document)
    {
        document["servers"] = new JsonArray(new JsonObject { ["url"] = string.Empty });
    }

    private static void RewritePaths(JsonObject document, string routePrefix)
    {
        if (document["paths"] is not JsonObject paths)
        {
            return;
        }

        var rewrittenPaths = new JsonObject();

        foreach (var path in paths.ToArray())
        {
            var rewrittenPath = RewritePath(path.Key, routePrefix);
            rewrittenPaths[rewrittenPath] = path.Value?.DeepClone();
        }

        document["paths"] = rewrittenPaths;
    }

    private static string RewritePath(string path, string routePrefix)
    {
        var normalizedPrefix = routePrefix.StartsWith('/')
            ? routePrefix
            : $"/{routePrefix}";

        if (path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
        {
            return $"{normalizedPrefix}/{path[5..]}";
        }

        if (string.Equals(path, "/api", StringComparison.OrdinalIgnoreCase))
        {
            return normalizedPrefix;
        }

        return $"{normalizedPrefix}{path}";
    }
}
