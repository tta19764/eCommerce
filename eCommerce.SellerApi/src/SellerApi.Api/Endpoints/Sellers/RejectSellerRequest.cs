namespace SellerApi.Api.Endpoints.Sellers;

/// <summary>Contains the reason for rejecting a seller application.</summary>
/// <param name="Reason">The administrator's reason. The value must contain at most 1,000 characters.</param>
public sealed record RejectSellerRequest(string Reason);
