namespace SOLID_Template.Domain.Entities;

/// <summary>
/// Product entity that represents a product in the system
/// </summary>
public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Sku { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    
    // Navigation Property - A product can be in multiple orders
    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
    
    /// <summary>
    /// Checks if the product is available for purchase
    /// </summary>
    public bool IsAvailable()
    {
        return IsActive && StockQuantity > 0;
    }
    
    /// <summary>
    /// Validates if the product has a valid SKU format
    /// </summary>
    public bool IsSkuValid()
    {
        return !string.IsNullOrEmpty(Sku) && 
               Sku.Length >= 3;
    }
    
    /// <summary>
    /// Calculates discount price based on percentage
    /// </summary>
    public decimal CalculateDiscountPrice(decimal discountPercentage)
    {
        if (discountPercentage < 0 || discountPercentage > 100)
            throw new ArgumentOutOfRangeException(nameof(discountPercentage), "Discount must be between 0 and 100");
            
        return Price * (1 - discountPercentage / 100);
    }
    
    /// <summary>
    /// Reduces stock quantity when product is sold
    /// </summary>
    public bool TryReduceStock(int quantity)
    {
        if (quantity <= 0 || quantity > StockQuantity)
            return false;
            
        StockQuantity -= quantity;
        return true;
    }
}