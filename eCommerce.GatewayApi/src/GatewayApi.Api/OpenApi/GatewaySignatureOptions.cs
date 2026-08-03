namespace GatewayApi.Api.OpenApi;

/// <summary>
/// Defines the GatewaySignatureOptions class used by this slice.
/// </summary>
public sealed class GatewaySignatureOptions
{
    public string HeaderName { get; init; } = string.Empty;

    public string Signature { get; init; } = string.Empty;
}
