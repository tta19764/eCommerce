namespace GatewayApi.Api.OpenApi;

/// <summary>
/// Defines the SwaggerServiceOptions class used by this slice.
/// </summary>
public sealed class SwaggerServiceOptions
{
    public List<SwaggerServiceDescriptor> Services { get; init; } = [];
}
