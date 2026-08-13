using SellerApi.Domain.Sellers;

namespace SellerApi.Application.Sellers.GetPendingSellers;

/// <summary>
/// Contains the information that an administrator needs to review a seller application.
/// </summary>
/// <param name="SellerId">The seller application identifier.</param>
/// <param name="Status">The current seller status.</param>
/// <param name="Applicant">The applicant profile.</param>
/// <param name="Store">The proposed store.</param>
/// <param name="SubmittedOnUtc">The time when the application was submitted.</param>
public sealed record PendingSellerApplicationResponse(
    Guid SellerId,
    SellerStatus Status,
    SellerApplicantResponse Applicant,
    ProposedStoreResponse Store,
    DateTime SubmittedOnUtc);

/// <summary>
/// Contains applicant profile details from UserApi.
/// </summary>
/// <param name="UserId">The UserApi identifier.</param>
/// <param name="FullName">The applicant's full name.</param>
/// <param name="Email">The applicant's email address.</param>
/// <param name="Found">Indicates whether UserApi found the profile.</param>
public sealed record SellerApplicantResponse(
    Guid UserId,
    string FullName,
    string Email,
    bool Found);

/// <summary>
/// Contains the proposed public store details.
/// </summary>
/// <param name="StoreId">The store identifier.</param>
/// <param name="Slug">The public store slug.</param>
/// <param name="Name">The public store name.</param>
/// <param name="Description">The public store description.</param>
/// <param name="CountryCode">The two-letter country code.</param>
/// <param name="DefaultCurrency">The three-letter default currency code.</param>
/// <param name="LogoImageId">The optional logo image identifier.</param>
/// <param name="BannerImageId">The optional banner image identifier.</param>
public sealed record ProposedStoreResponse(
    Guid StoreId,
    string Slug,
    string Name,
    string Description,
    string CountryCode,
    string DefaultCurrency,
    Guid? LogoImageId,
    Guid? BannerImageId);
