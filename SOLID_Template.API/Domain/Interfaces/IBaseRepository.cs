using SOLID_Template.Domain.Entities;

namespace SOLID_Template.Domain.Interfaces;

/// <summary>
/// Generic interface for basic repository operations
/// Implements Repository pattern following Dependency Inversion Principle (SOLID)
/// </summary>
/// <typeparam name="T">Entity type that inherits from BaseEntity</typeparam>
public interface IBaseRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> GetActiveAsync(); // Gets only non-deleted entities
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(Guid id); // Soft delete
    Task<bool> DeletePermanentlyAsync(Guid id); // Hard delete
    Task<bool> ExistsAsync(Guid id);
}