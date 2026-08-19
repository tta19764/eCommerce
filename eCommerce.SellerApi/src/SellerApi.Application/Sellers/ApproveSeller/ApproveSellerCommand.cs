using SharedLibrary.Application.Abstractions.Messaging;

namespace SellerApi.Application.Sellers.ApproveSeller;

/// <summary>Approves a pending seller application.</summary>
/// <param name="SellerId">The seller application identifier.</param>
/// <param name="AdminUserId">The UserApi identifier of the reviewing administrator.</param>
public sealed record ApproveSellerCommand(Guid SellerId, Guid AdminUserId) : ICommand;
