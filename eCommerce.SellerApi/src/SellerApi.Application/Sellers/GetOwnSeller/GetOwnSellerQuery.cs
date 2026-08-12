using SharedLibrary.Application.Abstractions.Messaging;

namespace SellerApi.Application.Sellers.GetOwnSeller;

/// <summary>Gets the seller application for one owner.</summary>
public sealed record GetOwnSellerQuery(Guid OwnerUserId) : IQuery<SellerResponse>;
