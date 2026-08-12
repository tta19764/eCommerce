namespace SellerApi.Api.Endpoints.Sellers;

/// <summary>Contains the reason for rejecting a seller application.</summary>
public sealed record RejectSellerRequest(string Reason);
