using SellerApi.Application.Stores;
using SharedLibrary.Application.Abstractions.Messaging;

namespace SellerApi.Application.Stores.GetStore;

/// <summary>Gets an active public store by slug.</summary>
/// <param name="Slug">The public slug. Lookup removes outer space and ignores casing.</param>
public sealed record GetStoreQuery(string Slug) : IQuery<StoreResponse>;
