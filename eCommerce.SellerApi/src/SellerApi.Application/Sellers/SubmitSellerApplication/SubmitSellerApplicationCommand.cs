using SharedLibrary.Application.Abstractions.Messaging;

namespace SellerApi.Application.Sellers.SubmitSellerApplication;

/// <summary>Creates a pending seller and store application.</summary>
/// <param name="OwnerUserId">The UserApi identifier of the applicant.</param>
/// <param name="Slug">The requested public store slug.</param>
/// <param name="Name">The requested public store name.</param>
/// <param name="Description">The requested public store description.</param>
/// <param name="CountryCode">The store's two-character country code.</param>
/// <param name="DefaultCurrency">The store's three-character default currency code.</param>
public sealed record SubmitSellerApplicationCommand(
    Guid OwnerUserId,
    string Slug,
    string Name,
    string Description,
    string CountryCode,
    string DefaultCurrency) : ICommand<Guid>;
