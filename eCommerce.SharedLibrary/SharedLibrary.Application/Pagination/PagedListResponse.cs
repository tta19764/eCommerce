namespace SharedLibrary.Application.Pagination;

/// <summary>
/// Paged read-model response.
/// </summary>
/// <param name="Items">Items in the requested page.</param>
/// <param name="Page">The one-based page number.</param>
/// <param name="PageSize">The maximum number of items requested.</param>
/// <param name="TotalCount">The total number of matching items.</param>
/// <typeparam name="T">The item response type.</typeparam>
public sealed record PagedListResponse<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    int TotalCount);
