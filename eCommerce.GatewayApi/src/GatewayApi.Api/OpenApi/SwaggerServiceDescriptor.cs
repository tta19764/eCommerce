namespace GatewayApi.Api.OpenApi;

/// <summary>
/// Defines the SwaggerServiceDescriptor class used by this slice.
/// </summary>
public sealed class SwaggerServiceDescriptor
{
    public string Name { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string Address { get; init; } = string.Empty;

    public string RoutePrefix { get; init; } = string.Empty;

    public string DocumentPath { get; init; } = "/openapi/v1.json";
}
