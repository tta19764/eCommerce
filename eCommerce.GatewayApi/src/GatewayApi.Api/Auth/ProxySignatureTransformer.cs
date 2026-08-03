using Yarp.ReverseProxy.Transforms;

namespace GatewayApi.Api.Auth;

internal sealed class ProxySignatureTransformer(
    string headerName,
    string signature) : RequestTransform
{
    /// <summary>
    /// Executes the ApplyAsync operation.
    /// </summary>
    /// <param name="context">The context value.</param>
    public override ValueTask ApplyAsync(RequestTransformContext context)
    {
        if (!string.IsNullOrWhiteSpace(headerName) &&
            !string.IsNullOrWhiteSpace(signature))
        {
            context.ProxyRequest.Headers.Remove(headerName);
            context.ProxyRequest.Headers.TryAddWithoutValidation(headerName, signature);
        }

        return ValueTask.CompletedTask;
    }
}
