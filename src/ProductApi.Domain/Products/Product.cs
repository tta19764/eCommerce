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
        IReadOnlyCollection<Guid>? imageIds)
        : base(id)
    {
        Name = name;
        Description = description;
        Price = price;
        Quantity = quantity;
        ImageIds = imageIds?.Distinct().ToArray() ?? [];
        Rating = 0.0m;
        ReviewsCount = 0;
    }
    
    public Name Name { get; private set; }

    public Description Description { get; private set; }

    public Money Price { get; private set; }

    public Quantity Quantity { get; private set; }

    public Guid[] ImageIds { get; private set; }

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
        IReadOnlyCollection<Guid>? imageIds = null)
    {
        // Products cannot be sold without a positive price.
        if(price.Amount <= 0)
            return Result.Failure<Product>(ProductErrors.InvalidPrice);
        
        // Negative stock would make availability and checkout decisions invalid.
        if(quantity.Value < 0)
            return Result.Failure<Product>(ProductErrors.InvalidQuantity);
        
        var product = new Product(Guid.NewGuid(), name, description, price, quantity, imageIds);
        
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
        IReadOnlyCollection<Guid>? imageIds = null)
    {
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

        Name = name;
        Description = description;
        Price = price;
        Quantity = quantity;
        ImageIds = imageIds?.Distinct().ToArray() ?? [];

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
}
