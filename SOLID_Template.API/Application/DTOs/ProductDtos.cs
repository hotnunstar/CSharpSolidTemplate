using System.ComponentModel.DataAnnotations;

namespace SOLID_Template.Application.DTOs;

/// <summary>
/// Product data transfer object for read operations
/// </summary>
public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Sku { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsAvailable { get; set; } // Calculated property
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating new products
/// </summary>
public class CreateProductDto
{
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 200 characters")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string Description { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }
    
    [Required(ErrorMessage = "SKU is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "SKU must be between 3 and 50 characters")]
    [RegularExpression(@"^[A-Z0-9\-_]+$", ErrorMessage = "SKU must contain only uppercase letters, numbers, hyphens and underscores")]
    public string Sku { get; set; } = string.Empty;
    
    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
    public int StockQuantity { get; set; }
    
    [Required(ErrorMessage = "Category is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Category must be between 2 and 100 characters")]
    public string Category { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO for updating existing products
/// </summary>
public class UpdateProductDto
{
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 200 characters")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string Description { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }
    
    [Required(ErrorMessage = "SKU is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "SKU must be between 3 and 50 characters")]
    [RegularExpression(@"^[A-Z0-9\-_]+$", ErrorMessage = "SKU must contain only uppercase letters, numbers, hyphens and underscores")]
    public string Sku { get; set; } = string.Empty;
    
    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
    public int StockQuantity { get; set; }
    
    [Required(ErrorMessage = "Category is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Category must be between 2 and 100 characters")]
    public string Category { get; set; } = string.Empty;
    
    public bool IsActive { get; set; }
}

/// <summary>
/// DTO for updating stock quantity
/// </summary>
public class UpdateStockDto
{
    [Required(ErrorMessage = "Quantity is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public int Quantity { get; set; }
    
    [Required(ErrorMessage = "Operation is required")]
    [RegularExpression(@"^(add|subtract)$", ErrorMessage = "Operation must be 'add' or 'subtract'")]
    public string Operation { get; set; } = "add"; // "add" or "subtract"
}