using ProductApi.Domain.Products;
using SharedLibrary.Infrastructure.Repositories;

namespace ProductApi.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for products.
/// </summary>
public class ProductRepository(ProductDbContext dbContext) : Repository<Product, ProductDbContext>(dbContext), IProductRepository
{
    
}
