namespace GatewayApi.Api.OpenApi;

/// <summary>
/// Configuration options for the API Gateway header signature used to authenticate downstream proxy requests.
/// </summary>
public sealed class GatewaySignatureOptions
{
    /// <summary>Gets or initializes the HTTP header name carrying the gateway signature.</summary>
    public string HeaderName { get; init; } = string.Empty;

    /// <summary>Gets or initializes the secret signature string value.</summary>
    public string Signature { get; init; } = string.Empty;
}

