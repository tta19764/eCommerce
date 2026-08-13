using Asp.Versioning;

namespace PaymentApi.Api.Endpoints;

/// <summary>Defines supported PaymentApi versions.</summary>
public static class PaymentApiApiVersions
{
    /// <summary>Gets version 1 of PaymentApi.</summary>
    public static ApiVersion V1 { get; } = new(1);
}
