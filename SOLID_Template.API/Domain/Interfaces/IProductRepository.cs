using SOLID_Template.Domain.Entities;

namespace SOLID_Template.Domain.Interfaces;

/// <summary>
/// Repository contract for Product operations
/// </summary>
public interface IProductRepository : IBaseRepository<Product>
{
    Task<Product?> GetBySkuAsync(string sku);
    Task<IEnumerable<Product>> GetByCategoryAsync(string category);
    Task<IEnumerable<Product>> GetAvailableProductsAsync();
    Task<IEnumerable<Product>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice);
    Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10);
    Task<bool> IsSkuUniqueAsync(string sku, Guid? excludeProductId = null);
}