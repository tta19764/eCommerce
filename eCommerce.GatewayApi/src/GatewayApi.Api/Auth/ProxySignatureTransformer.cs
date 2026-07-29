using Yarp.ReverseProxy.Transforms;

namespace GatewayApi.Api.Auth;

internal sealed class ProxySignatureTransformer(
    string headerName,
    string signature) : RequestTransform
{
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
