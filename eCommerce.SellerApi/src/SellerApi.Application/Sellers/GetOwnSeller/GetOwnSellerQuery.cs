using SharedLibrary.Application.Abstractions.Messaging;

namespace SellerApi.Application.Sellers.GetOwnSeller;

/// <summary>Gets the seller that the current user can manage.</summary>
/// <param name="OwnerUserId">The current UserApi identifier.</param>
/// <param name="IsAdmin">Indicates whether the current user is an administrator.</param>
public sealed record GetOwnSellerQuery(Guid OwnerUserId, bool IsAdmin) : IQuery<SellerResponse>;
