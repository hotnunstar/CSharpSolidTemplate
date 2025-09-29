namespace SOLID_Template.Domain.Entities;

/// <summary>
/// Base class for all domain entities
/// Implements common properties like Id, CreatedAt, UpdatedAt
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
}