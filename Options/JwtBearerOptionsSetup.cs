using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace eCommerce.SharedLibrary.Options;

public sealed class JwtBearerOptionsSetup(IOptions<AuthenticationOptions> options)
    : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly AuthenticationOptions _options = options.Value;

    public void Configure(JwtBearerOptions options)
    {
        options.Audience = _options.Audience;
        options.TokenValidationParameters.ValidIssuer = _options.Issuer;
        options.MetadataAddress = _options.MetadataUrl;
        options.RequireHttpsMetadata = _options.RequireHttpsMetadata;
    }

    public void Configure(string? name, JwtBearerOptions options)
    {
        Configure(options);
    }
}