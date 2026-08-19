using SellerApi.Domain.Sellers;
using SharedLibrary.Domain.Abstractions;

namespace SellerApi.Domain.Stores;

/// <summary>
/// Represents the public store of one seller.
/// </summary>
public sealed class Store : Entity
{
    private Store()
    {
        Slug = null!;
        Name = null!;
        Description = null!;
        CountryCode = null!;
        DefaultCurrency = null!;
    }

    private Store(
        Guid id,
        Guid sellerId,
        string slug,
        string name,
        string description,
        string countryCode,
        string defaultCurrency,
        DateTime createdOnUtc)
        : base(id)
    {
        SellerId = sellerId;
        Slug = slug;
        Name = name;
        Description = description;
        CountryCode = countryCode;
        DefaultCurrency = defaultCurrency;
        CreatedOnUtc = createdOnUtc;
    }

    /// <summary>Gets the identifier of the seller that owns the store.</summary>
    public Guid SellerId { get; private set; }

    /// <summary>Gets the unique public store slug.</summary>
    public string Slug { get; private set; }

    /// <summary>Gets the public store name.</summary>
    public string Name { get; private set; }

    /// <summary>Gets the public store description.</summary>
    public string Description { get; private set; }

    /// <summary>Gets the two-letter store country code.</summary>
    public string CountryCode { get; private set; }

    /// <summary>Gets the three-letter default currency code.</summary>
    public string DefaultCurrency { get; private set; }

    /// <summary>Gets the optional logo image identifier.</summary>
    public Guid? LogoImageId { get; private set; }

    /// <summary>Gets the optional banner image identifier.</summary>
    public Guid? BannerImageId { get; private set; }

    /// <summary>Gets the sum of all review ratings.</summary>
    public long RatingSum { get; private set; }

    /// <summary>Gets the number of store reviews.</summary>
    public int ReviewCount { get; private set; }

    /// <summary>Gets the average store rating.</summary>
    public decimal AverageRating => ReviewCount == 0
        ? 0
        : decimal.Round((decimal)RatingSum / ReviewCount, 2);

    /// <summary>Gets the time when the store application was created.</summary>
    public DateTime CreatedOnUtc { get; private set; }

    /// <summary>Creates the proposed store for a seller application.</summary>
    /// <param name="sellerId">The identifier of the seller that owns the store. It must not be empty.</param>
    /// <param name="slug">The public slug. Its normalized value must contain 3 through 80 ASCII letters, digits, or hyphens.</param>
    /// <param name="name">The public name. Its trimmed length must be 2 through 120 characters.</param>
    /// <param name="description">The public description. Its trimmed length must not exceed 2,000 characters.</param>
    /// <param name="countryCode">The country code. Its trimmed value must contain two characters.</param>
    /// <param name="defaultCurrency">The default currency code. Its trimmed value must contain three characters.</param>
    /// <param name="createdOnUtc">The UTC creation time.</param>
    /// <returns>The new store, or a validation failure.</returns>
    /// <remarks>The factory trims all text, lowercases the slug, and uppercases the country and currency codes.</remarks>
    public static Result<Store> Create(
        Guid sellerId,
        string slug,
        string name,
        string description,
        string countryCode,
        string defaultCurrency,
        DateTime createdOnUtc)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();
        var normalizedName = name.Trim();
        var normalizedDescription = description.Trim();
        var normalizedCountry = countryCode.Trim().ToUpperInvariant();
        var normalizedCurrency = defaultCurrency.Trim().ToUpperInvariant();

        var slugIsValid = normalizedSlug.Length is >= 3 and <= 80
            && normalizedSlug.All(character => char.IsAsciiLetterOrDigit(character) || character == '-');

        if (sellerId == Guid.Empty
            || !slugIsValid
            || normalizedName.Length is < 2 or > 120
            || normalizedDescription.Length > 2000
            || normalizedCountry.Length != 2
            || normalizedCurrency.Length != 3)
        {
            return Result.Failure<Store>(StoreErrors.Invalid);
        }

        return Result.Success(new Store(
            Guid.NewGuid(),
            sellerId,
            normalizedSlug,
            normalizedName,
            normalizedDescription,
            normalizedCountry,
            normalizedCurrency,
            createdOnUtc));
    }

    /// <summary>Adds one rating to the store rating summary.</summary>
    /// <param name="rating">The rating from 1 through 5.</param>
    /// <remarks>The caller must validate the range and persist this change in the same transaction as the review.</remarks>
    public void AddRating(byte rating)
    {
        RatingSum += rating;
        ReviewCount++;
    }

    /// <summary>Removes one rating from the store rating summary.</summary>
    /// <param name="rating">The rating to remove.</param>
    /// <remarks>The caller must supply an existing rating and persist this change with the review removal.</remarks>
    public void RemoveRating(byte rating)
    {
        RatingSum -= rating;
        ReviewCount--;
    }
}
