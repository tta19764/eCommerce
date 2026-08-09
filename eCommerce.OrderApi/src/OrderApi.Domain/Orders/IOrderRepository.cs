using System.Linq.Expressions;

namespace OrderApi.Domain.Orders;

/// <summary>
/// Repository abstraction for order persistence.
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// Gets the first order that matches the supplied predicate.
    /// </summary>
    /// <param name="predicate">The order filter expression.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The matching order, or null when no order matches.</returns>
    public Task<Order?> GetByAsync(Expression<Func<Order, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a tracked order aggregate by identifier for reading or mutation through domain methods.
    /// </summary>
    /// <param name="id">The order identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The matching order, or null when no order exists.</returns>
    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all orders.
    /// </summary>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>All orders.</returns>
    public Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets one page of orders.
    /// </summary>
    /// <param name="page">The one-based page number.</param>
    /// <param name="pageSize">The maximum number of orders to return.</param>
    /// <param name="minOrderPrice">The minimum total order price to include.</param>
    /// <param name="maxOrderPrice">The maximum total order price to include.</param>
    /// <param name="sortByOrderPrice">Sorts by total order price when true; otherwise sorts by newest first.</param>
    /// <param name="sortDescending">Sorts descending when true; otherwise ascending.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The requested page of orders.</returns>
    public Task<IEnumerable<Order>> GetPageAsync(
        int page = 1,
        int pageSize = 10,
        decimal? minOrderPrice = null,
        decimal? maxOrderPrice = null,
        bool sortByOrderPrice = false,
        bool sortDescending = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts orders matching the supplied optional total-price filters.
    /// </summary>
    /// <param name="minOrderPrice">The minimum total order price to include.</param>
    /// <param name="maxOrderPrice">The maximum total order price to include.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The number of matching orders.</returns>
    public Task<int> CountAsync(
        decimal? minOrderPrice = null,
        decimal? maxOrderPrice = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets orders placed by the supplied client.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="page">The one-based page number.</param>
    /// <param name="pageSize">The maximum number of orders to return.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>Orders for the client sorted by newest first.</returns>
    public Task<IEnumerable<Order>> GetOrdersByClientId(
        Guid clientId,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts orders placed by the supplied client.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The number of matching client orders.</returns>
    public Task<int> CountByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a tracked order aggregate that contains the supplied seller-order group.
    /// </summary>
    public Task<Order?> GetBySellerOrderIdAsync(Guid sellerOrderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets seller-order groups for one seller.
    /// </summary>
    public Task<IEnumerable<Order>> GetOrdersBySellerIdAsync(
        Guid sellerId,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts seller-order groups for one seller.
    /// </summary>
    public Task<int> CountBySellerIdAsync(Guid sellerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an order for deletion.
    /// </summary>
    /// <param name="order">The order to delete.</param>
    public void Delete(Order order);

    /// <summary>
    /// Marks an order for insertion.
    /// </summary>
    /// <param name="order">The order to add.</param>
    public void Add(Order order);

    /// <summary>
    /// Checks purchase status for a client and product.
    /// </summary>
    public Task<(bool HasPurchased, bool HasCompletedOrder)> GetPurchaseStatusAsync(
        Guid clientId,
        Guid productId,
        CancellationToken cancellationToken = default);
}
