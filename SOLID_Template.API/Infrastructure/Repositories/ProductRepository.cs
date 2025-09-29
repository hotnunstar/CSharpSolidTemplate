using Microsoft.EntityFrameworkCore;
using SOLID_Template.Domain.Entities;
using SOLID_Template.Domain.Interfaces;
using SOLID_Template.Infrastructure.Data;

namespace SOLID_Template.Infrastructure.Repositories;

/// <summary>
/// Specific implementation of repository for Product entity
/// Extends BaseRepository and implements IProductRepository
/// </summary>
public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Product?> GetBySkuAsync(string sku)
    {
        return await _context.Products
            .Where(p => !p.IsDeleted && p.Sku.ToLower() == sku.ToLower())
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(string category)
    {
        return await _context.Products
            .Where(p => !p.IsDeleted && p.Category.ToLower() == category.ToLower())
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetAvailableProductsAsync()
    {
        return await _context.Products
            .Where(p => !p.IsDeleted && p.IsActive && p.StockQuantity > 0)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        return await _context.Products
            .Where(p => !p.IsDeleted && p.Price >= minPrice && p.Price <= maxPrice)
            .OrderBy(p => p.Price)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10)
    {
        return await _context.Products
            .Where(p => !p.IsDeleted && p.IsActive && p.StockQuantity <= threshold)
            .OrderBy(p => p.StockQuantity)
            .ToListAsync();
    }

    public async Task<bool> IsSkuUniqueAsync(string sku, Guid? excludeProductId = null)
    {
        var query = _context.Products
            .Where(p => !p.IsDeleted && p.Sku.ToLower() == sku.ToLower());

        if (excludeProductId.HasValue)
        {
            query = query.Where(p => p.Id != excludeProductId.Value);
        }

        return !await query.AnyAsync();
    }
}