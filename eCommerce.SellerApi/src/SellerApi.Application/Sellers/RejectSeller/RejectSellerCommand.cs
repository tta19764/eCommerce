using SharedLibrary.Application.Abstractions.Messaging;

namespace SellerApi.Application.Sellers.RejectSeller;

/// <summary>Rejects a pending seller application.</summary>
/// <param name="SellerId">The seller application identifier.</param>
/// <param name="AdminUserId">The UserApi identifier of the reviewing administrator.</param>
/// <param name="Reason">The rejection reason. The trimmed value must not be empty or exceed 1,000 characters.</param>
public sealed record RejectSellerCommand(Guid SellerId, Guid AdminUserId, string Reason) : ICommand;
