namespace SOLID_Template.Domain.Entities;

/// <summary>
/// Association entity between Order and Product (many-to-many)
/// Allows an order to have multiple products with quantities and a product to be in multiple orders
/// </summary>
public class OrderProduct
{
    public Guid OrderId { get; set; }
    public virtual Order Order { get; set; } = null!;
    
    public Guid ProductId { get; set; }
    public virtual Product Product { get; set; } = null!;
    
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; } // Price at the time of order
    public decimal Discount { get; set; } = 0; // Discount applied to this product in the order
    public DateTime AddedDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Calculates the total price for this product in the order (quantity * unit price - discount)
    /// </summary>
    public decimal GetTotalPrice()
    {
        return (Quantity * UnitPrice) - Discount;
    }
}