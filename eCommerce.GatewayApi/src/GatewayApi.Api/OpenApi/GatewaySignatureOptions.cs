namespace GatewayApi.Api.OpenApi;

public sealed class GatewaySignatureOptions
{
    public string HeaderName { get; init; } = string.Empty;

    public string Signature { get; init; } = string.Empty;
}
