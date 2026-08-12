using SharedLibrary.Application.Abstractions.Messaging;

namespace SellerApi.Application.Sellers.SubmitSellerApplication;

/// <summary>Creates a pending seller and store application.</summary>
public sealed record SubmitSellerApplicationCommand(
    Guid OwnerUserId,
    string Slug,
    string Name,
    string Description,
    string CountryCode,
    string DefaultCurrency) : ICommand<Guid>;
