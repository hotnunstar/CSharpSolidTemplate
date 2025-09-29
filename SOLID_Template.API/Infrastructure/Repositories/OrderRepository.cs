using Microsoft.EntityFrameworkCore;
using SOLID_Template.Domain.Entities;
using SOLID_Template.Domain.Interfaces;
using SOLID_Template.Infrastructure.Data;

namespace SOLID_Template.Infrastructure.Repositories;

/// <summary>
/// Specific implementation of repository for Order entity
/// Inherits from BaseRepository and implements business-specific methods
/// </summary>
public class OrderRepository : BaseRepository<Order>, IOrderRepository
{
    public OrderRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Order?> GetByNumberAsync(string number)
    {
        return await _dbSet
            .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
            .FirstOrDefaultAsync(o => o.Number == number && !o.IsDeleted);
    }

    public async Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status)
    {
        return await _dbSet
            .Where(o => o.Status == status && !o.IsDeleted)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(o => o.OrderDate.Date >= startDate.Date && 
                       o.OrderDate.Date <= endDate.Date && 
                       !o.IsDeleted)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetByProductIdAsync(Guid productId)
    {
        return await _dbSet
            .Where(o => o.OrderProducts.Any(op => op.ProductId == productId) && !o.IsDeleted)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<Order?> GetWithProductsAsync(Guid id)
    {
        return await _dbSet
            .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
            .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);
    }

    public async Task<bool> NumberExistsAsync(string number)
    {
        return await _dbSet
            .AnyAsync(o => o.Number == number && !o.IsDeleted);
    }

    public override async Task<Order?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public override async Task<IEnumerable<Order>> GetAllAsync()
    {
        return await _dbSet
            .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
            .ToListAsync();
    }

    public override async Task<IEnumerable<Order>> GetActiveAsync()
    {
        return await _dbSet
            .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
            .Where(x => !x.IsDeleted)
            .ToListAsync();
    }
}