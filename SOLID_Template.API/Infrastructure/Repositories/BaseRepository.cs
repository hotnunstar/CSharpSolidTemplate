using Microsoft.EntityFrameworkCore;
using SOLID_Template.Domain.Entities;
using SOLID_Template.Domain.Interfaces;
using SOLID_Template.Infrastructure.Data;

namespace SOLID_Template.Infrastructure.Repositories;

/// <summary>
/// Implementação genérica do repositório base
/// Implementa operações CRUD comuns seguindo o padrão Repository
/// </summary>
/// <typeparam name="T">Tipo da entidade que herda de BaseEntity</typeparam>
public class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public BaseRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> GetActiveAsync()
    {
        return await _dbSet.Where(x => !x.IsDeleted).ToListAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null || entity.IsDeleted)
            return false;

        // Soft delete
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await UpdateAsync(entity);
        return true;
    }

    public virtual async Task<bool> DeletePermanentlyAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null)
            return false;

        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public virtual async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.AnyAsync(x => x.Id == id && !x.IsDeleted);
    }
}