using SellerApi.Application.Sellers;
using SharedLibrary.Application.Abstractions.Messaging;

namespace SellerApi.Application.Stores.GetStore;

/// <summary>Gets an active public store by slug.</summary>
public sealed record GetStoreQuery(string Slug) : IQuery<StoreResponse>;
