namespace SharedLibrary.Infrastructure.Options;

public sealed class GatewayOptions
{
    public required string HeaderName { get; set; }
    
    public required string Signature { get; set; }
}