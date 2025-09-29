using SOLID_Template.Domain.Entities;

namespace SOLID_Template.Domain.Interfaces;

/// <summary>
/// Specific interface for Order entity repository operations
/// Extends IBaseRepository and adds business-specific methods
/// </summary>
public interface IOrderRepository : IBaseRepository<Order>
{
    Task<Order?> GetByNumberAsync(string number);
    Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status);
    Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Order>> GetByProductIdAsync(Guid productId);
    Task<Order?> GetWithProductsAsync(Guid id); // Includes related products
    Task<bool> NumberExistsAsync(string number);
}