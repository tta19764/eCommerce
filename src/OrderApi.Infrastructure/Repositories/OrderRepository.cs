using Microsoft.EntityFrameworkCore;
using OrderApi.Domain.Orders;
using SharedLibrary.Infrastructure.Repositories;

namespace OrderApi.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for orders.
/// </summary>
public class OrderRepository(OrderDbContext dbContext) : Repository<Order, OrderDbContext>(dbContext), IOrderRepository
{
    public new async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(order => order.Items)
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);
    }

    /// <summary>
    /// Gets one page of orders with items loaded and newest orders first.
    /// </summary>
    /// <param name="page">The one-based page number.</param>
    /// <param name="pageSize">The maximum number of orders to return.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The requested page of orders.</returns>
    public override async Task<IEnumerable<Order>> GetPageAsync(
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        return await GetPageAsync(
            page,
            pageSize,
            null,
            null,
            false,
            true,
            cancellationToken);
    }

    /// <summary>
    /// Gets one page of orders with optional total price filtering and sorting.
    /// </summary>
    /// <param name="page">The one-based page number.</param>
    /// <param name="pageSize">The maximum number of orders to return.</param>
    /// <param name="minOrderPrice">The minimum total order price to include.</param>
    /// <param name="maxOrderPrice">The maximum total order price to include.</param>
    /// <param name="sortByOrderPrice">Sorts by total order price when true; otherwise sorts by newest first.</param>
    /// <param name="sortDescending">Sorts descending when true; otherwise ascending.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The requested page of orders.</returns>
    public async Task<IEnumerable<Order>> GetPageAsync(
        int page = 1,
        int pageSize = 10,
        decimal? minOrderPrice = null,
        decimal? maxOrderPrice = null,
        bool sortByOrderPrice = false,
        bool sortDescending = true,
        CancellationToken cancellationToken = default)
    {
        var query = FilterByOrderPrice(
            DbSet
            .AsNoTracking()
            .Include(order => order.Items),
            minOrderPrice,
            maxOrderPrice);

        return await ApplyOrdering(query, sortByOrderPrice, sortDescending)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets orders placed by the supplied client with newest orders first.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="page">The one-based page number.</param>
    /// <param name="pageSize">The maximum number of orders to return.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>Orders for the client.</returns>
    public async Task<IEnumerable<Order>> GetOrdersByClientId(
        Guid clientId,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(order => order.Items)
            .Where(order => order.ClientId == clientId)
            .OrderByDescending(order => order.CreatedAtUtc.Value)
            .ThenByDescending(order => order.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<Order> FilterByOrderPrice(
        IQueryable<Order> query,
        decimal? minOrderPrice,
        decimal? maxOrderPrice)
    {
        if (minOrderPrice.HasValue)
        {
            query = query.Where(order => order.Items.Sum(item => item.UnitPrice.Amount * item.Quantity.Value) >= minOrderPrice.Value);
        }

        if (maxOrderPrice.HasValue)
        {
            query = query.Where(order => order.Items.Sum(item => item.UnitPrice.Amount * item.Quantity.Value) <= maxOrderPrice.Value);
        }

        return query;
    }

    private static IOrderedQueryable<Order> ApplyOrdering(
        IQueryable<Order> query,
        bool sortByOrderPrice,
        bool sortDescending)
    {
        if (sortByOrderPrice)
        {
            return sortDescending
                ? query.OrderByDescending(order => order.Items.Sum(item => item.UnitPrice.Amount * item.Quantity.Value))
                    .ThenByDescending(order => order.CreatedAtUtc.Value)
                    .ThenByDescending(order => order.Id)
                : query.OrderBy(order => order.Items.Sum(item => item.UnitPrice.Amount * item.Quantity.Value))
                    .ThenByDescending(order => order.CreatedAtUtc.Value)
                    .ThenByDescending(order => order.Id);
        }

        return sortDescending
            ? query.OrderByDescending(order => order.CreatedAtUtc.Value)
                .ThenByDescending(order => order.Id)
            : query.OrderBy(order => order.CreatedAtUtc.Value)
                .ThenBy(order => order.Id);
    }

}
