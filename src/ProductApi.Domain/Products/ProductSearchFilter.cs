namespace ProductApi.Domain.Products;

/// <summary>
/// Product catalog search and filtering criteria.
/// </summary>
public sealed record ProductSearchFilter(
    int Page,
    int PageSize,
    string? SearchTerm,
    IReadOnlyCollection<Guid>? CategoryIds,
    ProductType? ProductType,
    Guid? SellerId,
    decimal? MinPrice,
    decimal? MaxPrice,
    decimal? MinRating,
    bool? InStock,
    ProductSortBy SortBy,
    bool SortDescending);
