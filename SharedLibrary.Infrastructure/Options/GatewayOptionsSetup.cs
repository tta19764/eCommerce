using Microsoft.Extensions.Options;

namespace SharedLibrary.Infrastructure.Options;

public class GatewayOptionsSetup(IOptions<GatewayOptions> options) : IConfigureOptions<GatewayOptions>
{
    private readonly GatewayOptions _options = options.Value;

    public void Configure(GatewayOptions options)
    {
        options.HeaderName = _options.HeaderName;
        options.Signature = _options.Signature;
    }
    
    public void Configure(string? name, GatewayOptions options)
    {
        Configure(options);
    }
}