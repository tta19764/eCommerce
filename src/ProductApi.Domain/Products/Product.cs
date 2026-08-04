using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Domain.Money;

namespace ProductApi.Domain.Products;

/// <summary>
/// Product aggregate root.
/// </summary>
public class Product : Entity
{
    private Product()
    {
        Name = null!;
        Description = null!;
        Price = null!;
        Quantity = null!;
        ImageIds = [];
    }

    private Product(
        Guid id, 
        Name name,
        Description description,
        Money price, 
        Quantity quantity,
        IReadOnlyCollection<Guid>? imageIds,
        Guid? displayImageId,
        Guid sellerId,
        Guid categoryId,
        ProductType productType)
        : base(id)
    {
        Name = name;
        Description = description;
        Price = price;
        Quantity = quantity;
        ImageIds = imageIds?.Distinct().ToArray() ?? [];
        DisplayImageId = displayImageId;
        SellerId = sellerId;
        CategoryId = categoryId;
        ProductType = productType;
        Rating = 0.0m;
        ReviewsCount = 0;
    }
    
    public Name Name { get; private set; }

    public Description Description { get; private set; }

    public Money Price { get; private set; }

    public Quantity Quantity { get; private set; }

    public Guid[] ImageIds { get; private set; }

    public Guid? DisplayImageId { get; private set; }

    /// <summary>
    /// Seller account or profile that owns this marketplace listing.
    /// </summary>
    public Guid SellerId { get; private set; }

    /// <summary>
    /// Primary marketplace category used for browsing and filtering.
    /// </summary>
    public Guid CategoryId { get; private set; }

    /// <summary>
    /// Fulfillment type for this listing.
    /// </summary>
    public ProductType ProductType { get; private set; }

    /// <summary>
    /// Average product rating rounded to one digit after the decimal point.
    /// </summary>
    public decimal Rating { get; private set; }

    /// <summary>
    /// Number of reviews included in the rating.
    /// </summary>
    public int ReviewsCount { get; private set; }
    
    /// <summary>
    /// Creates a product when the supplied values satisfy product invariants.
    /// </summary>
    /// <param name="name">The product name.</param>
    /// <param name="price">The product price.</param>
    /// <param name="quantity">The available product quantity.</param>
    /// <returns>A successful result containing the product, or a failure result with a product error.</returns>
    public static Result<Product> Create( 
        Name name,
        Description description,
        Money price, 
        Quantity quantity,
        IReadOnlyCollection<Guid>? imageIds = null,
        Guid? displayImageId = null,
        Guid? sellerId = null,
        Guid? categoryId = null,
        ProductType productType = ProductType.Physical)
    {
        if (sellerId is null || sellerId == Guid.Empty)
            return Result.Failure<Product>(ProductErrors.InvalidSeller);

        if (categoryId is null || categoryId == Guid.Empty)
            return Result.Failure<Product>(ProductErrors.InvalidCategory);

        // Products cannot be sold without a positive price.
        if(price.Amount <= 0)
            return Result.Failure<Product>(ProductErrors.InvalidPrice);
        
        // Negative stock would make availability and checkout decisions invalid.
        if(quantity.Value < 0)
            return Result.Failure<Product>(ProductErrors.InvalidQuantity);

        var distinctImageIds = imageIds?.Distinct().ToArray() ?? [];
        var resolvedDisplayImageId = ResolveDisplayImageId(distinctImageIds, displayImageId);

        // The display image is not a separate image; it must point at one of the product images.
        // Keeping this invariant in the aggregate protects every read model built from Product.
        if (resolvedDisplayImageId.IsFailure)
        {
            return Result.Failure<Product>(resolvedDisplayImageId.Error);
        }
        
        var product = new Product(
            Guid.NewGuid(),
            name,
            description,
            price,
            quantity,
            distinctImageIds,
            resolvedDisplayImageId.Value,
            sellerId.Value,
            categoryId.Value,
            productType);
        
        return product;
    }

    /// <summary>
    /// Updates product details when the supplied values satisfy product invariants.
    /// </summary>
    /// <param name="name">The product name.</param>
    /// <param name="price">The product price.</param>
    /// <param name="quantity">The available product quantity.</param>
    /// <returns>A success result, or a failure result with a product error.</returns>
    public Result Update(
        Name name,
        Description description,
        Money price,
        Quantity quantity,
        IReadOnlyCollection<Guid>? imageIds = null,
        Guid? displayImageId = null,
        Guid? categoryId = null,
        ProductType? productType = null)
    {
        if (categoryId is null || categoryId == Guid.Empty)
        {
            return Result.Failure(ProductErrors.InvalidCategory);
        }

        // Products cannot be sold without a positive price.
        if (price.Amount <= 0)
        {
            return Result.Failure(ProductErrors.InvalidPrice);
        }

        // Negative stock would make availability and checkout decisions invalid.
        if (quantity.Value < 0)
        {
            return Result.Failure(ProductErrors.InvalidQuantity);
        }

        var distinctImageIds = imageIds?.Distinct().ToArray() ?? [];
        var resolvedDisplayImageId = ResolveDisplayImageId(distinctImageIds, displayImageId);

        // Re-evaluate the display image after every image-list replacement, otherwise an update
        // could leave DisplayImageId pointing at an image no longer attached to the product.
        if (resolvedDisplayImageId.IsFailure)
        {
            return Result.Failure(resolvedDisplayImageId.Error);
        }

        Name = name;
        Description = description;
        Price = price;
        Quantity = quantity;
        ImageIds = distinctImageIds;
        DisplayImageId = resolvedDisplayImageId.Value;
        CategoryId = categoryId.Value;
        ProductType = productType ?? ProductType;

        return Result.Success();
    }

    /// <summary>
    /// Applies a newly created review to the denormalized rating summary.
    /// </summary>
    /// <param name="rating">The new review rating from one to five.</param>
    /// <returns>A success result, or a validation failure.</returns>
    public Result AddReview(int rating)
    {
        if (rating is < 1 or > 5)
        {
            return Result.Failure(ProductErrors.InvalidReviewRating);
        }

        Rating = Math.Round(
            ((Rating * ReviewsCount) + rating) / (ReviewsCount + 1),
            1,
            MidpointRounding.AwayFromZero);
        ReviewsCount++;

        return Result.Success();
    }

    /// <summary>
    /// Executes the AdjustQuantity operation.
    /// </summary>
    /// <param name="quantityDelta">The quantityDelta value.</param>
    public Result AdjustQuantity(int quantityDelta)
    {
        var adjustedQuantity = Quantity.Value + quantityDelta;

        if (adjustedQuantity < 0)
        {
            return Result.Failure(ProductErrors.InsufficientQuantity);
        }

        Quantity = new Quantity(adjustedQuantity);

        return Result.Success();
    }

    private static Result<Guid?> ResolveDisplayImageId(Guid[] imageIds, Guid? displayImageId)
    {
        if (imageIds.Length == 0)
        {
            return displayImageId is null
                ? Result.Success<Guid?>(null)
                : Result.Failure<Guid?>(ProductErrors.InvalidDisplayImage);
        }

        var resolvedDisplayImageId = displayImageId ?? imageIds[0];

        return imageIds.Contains(resolvedDisplayImageId)
            ? Result.Success<Guid?>(resolvedDisplayImageId)
            : Result.Failure<Guid?>(ProductErrors.InvalidDisplayImage);
    }
}
