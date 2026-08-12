using SharedLibrary.Application.Abstractions.Messaging;

namespace SellerApi.Application.Sellers.ApproveSeller;

/// <summary>Approves a pending seller application.</summary>
public sealed record ApproveSellerCommand(Guid SellerId, Guid AdminUserId) : ICommand;
