using SellerApi.Domain.Stores;

namespace SellerApi.Domain.Sellers;

/// <summary>
/// Contains a pending seller and its proposed store.
/// </summary>
/// <param name="Seller">The pending seller.</param>
/// <param name="Store">The proposed store.</param>
public sealed record PendingSellerApplication(Seller Seller, Store Store);
