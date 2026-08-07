using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;

namespace GatewayApi.Api.OpenApi;

/// <summary>
/// Fetches, rewrites, and proxies downstream microservice OpenAPI specifications for unified Gateway Swagger rendering.
/// </summary>
public sealed class SwaggerDocumentProxy(
    IHttpClientFactory httpClientFactory,
    IOptions<SwaggerServiceOptions> options,
    IOptions<GatewaySignatureOptions> gatewayOptions)
{
    private const string BearerSecuritySchemeName = "Bearer";

    /// <summary>
    /// Fetches and transforms the OpenAPI specification for a configured downstream service.
    /// </summary>
    /// <param name="serviceName">The unique service name.</param>
    /// <param name="context">The HTTP context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The transformed OpenAPI JSON document or error problem details.</returns>
    public async Task<IResult> GetSwaggerDocumentAsync(
        string serviceName,
        HttpContext context,
        CancellationToken cancellationToken)

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
            using var request = new HttpRequestMessage(HttpMethod.Get, documentUrl);
            AddGatewaySignature(request);

            using var response = await httpClient.SendAsync(request, cancellationToken);

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
            RewriteServers(json, context);
            RewritePaths(json, service.RoutePrefix);
            AddBearerSecurity(json);

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

    private void AddGatewaySignature(HttpRequestMessage request)
    {
        var signatureOptions = gatewayOptions.Value;

        if (string.IsNullOrWhiteSpace(signatureOptions.HeaderName) ||
            string.IsNullOrWhiteSpace(signatureOptions.Signature))
        {
            return;
        }

        request.Headers.TryAddWithoutValidation(signatureOptions.HeaderName, signatureOptions.Signature);
    }

    private static void RewriteInfo(JsonObject document, SwaggerServiceDescriptor service)
    {
        var info = document["info"] as JsonObject ?? [];
        info["title"] = service.DisplayName;
        document["info"] = info;
    }

    private static void RewriteServers(JsonObject document, HttpContext context)
    {
        var gatewayUrl = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}";
        document["servers"] = new JsonArray(new JsonObject { ["url"] = gatewayUrl });
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

    private static void AddBearerSecurity(JsonObject document)
    {
        var components = document["components"] as JsonObject ?? [];
        var securitySchemes = components["securitySchemes"] as JsonObject ?? [];

        securitySchemes[BearerSecuritySchemeName] = new JsonObject
        {
            ["type"] = "http",
            ["scheme"] = "bearer",
            ["bearerFormat"] = "JWT",
            ["description"] = "Paste the access token returned from the authentication login or refresh endpoint."
        };

        components["securitySchemes"] = securitySchemes;
        document["components"] = components;

        if (document["paths"] is not JsonObject paths)
        {
            return;
        }

        foreach (var path in paths)
        {
            if (path.Value is not JsonObject pathItem)
            {
                continue;
            }

            foreach (var operation in pathItem)
            {
                if (operation.Value is JsonObject operationObject &&
                    RequiresBearerToken(operationObject))
                {
                    operationObject["security"] = new JsonArray
                    {
                        new JsonObject
                        {
                            [BearerSecuritySchemeName] = new JsonArray()
                        }
                    };
                }
            }
        }
    }

    private static bool RequiresBearerToken(JsonObject operation)
    {
        return operation["responses"] is JsonObject responses &&
            responses.ContainsKey(StatusCodes.Status401Unauthorized.ToString());
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
