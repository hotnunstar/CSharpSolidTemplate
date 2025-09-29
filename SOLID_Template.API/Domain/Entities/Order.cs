namespace SOLID_Template.Domain.Entities;

/// <summary>
/// Order entity that represents an order in the system
/// </summary>
public class Order : BaseEntity
{
    public string Number { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    
    // Navigation Property - An order can have multiple products
    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
    
    /// <summary>
    /// Calculates the number of products in the order
    /// </summary>
    public int GetProductCount()
    {
        return OrderProducts?.Count ?? 0;
    }
    
    /// <summary>
    /// Calculates the total quantity of all products in the order
    /// </summary>
    public int GetTotalQuantity()
    {
        return OrderProducts?.Sum(op => op.Quantity) ?? 0;
    }
    
    /// <summary>
    /// Calculates the total amount based on products
    /// </summary>
    public decimal CalculateTotalAmount()
    {
        return OrderProducts?.Sum(op => op.GetTotalPrice()) ?? 0;
    }
    
    /// <summary>
    /// Checks if the order can be canceled
    /// </summary>
    public bool CanCancel()
    {
        return Status == OrderStatus.Pending || Status == OrderStatus.Processing;
    }
    
    /// <summary>
    /// Cancels the order if possible
    /// </summary>
    public void Cancel()
    {
        if (CanCancel())
        {
            Status = OrderStatus.Canceled;
            UpdatedAt = DateTime.UtcNow;
        }
    }
    
    /// <summary>
    /// Approves the order
    /// </summary>
    public void Approve()
    {
        if (Status == OrderStatus.Pending)
        {
            Status = OrderStatus.Approved;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}