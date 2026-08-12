using SharedLibrary.Application.Abstractions.Messaging;

namespace SellerApi.Application.Sellers.RejectSeller;

/// <summary>Rejects a pending seller application.</summary>
public sealed record RejectSellerCommand(Guid SellerId, Guid AdminUserId, string Reason) : ICommand;
