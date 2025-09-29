using System.ComponentModel.DataAnnotations;
using SOLID_Template.Domain.Entities;

namespace SOLID_Template.Application.DTOs.Order;

/// <summary>
/// DTO for creating a new order
/// </summary>
public class CreateOrderDto
{
    [Required(ErrorMessage = "Order number is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Order number must be between 3 and 50 characters")]
    public string Number { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string Description { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "At least one product is required")]
    [MinLength(1, ErrorMessage = "Order must contain at least one product")]
    public List<OrderProductDto> Products { get; set; } = new();
}

/// <summary>
/// DTO for product in order
/// </summary>
public class OrderProductDto
{
    [Required(ErrorMessage = "Product ID is required")]
    public Guid ProductId { get; set; }
    
    [Required(ErrorMessage = "Quantity is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public int Quantity { get; set; }
    
    [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than 0")]
    public decimal? UnitPrice { get; set; } // Optional - will use current price if not provided
    
    [Range(0, double.MaxValue, ErrorMessage = "Discount cannot be negative")]
    public decimal Discount { get; set; } = 0;
}

/// <summary>
/// DTO for updating an existing order
/// </summary>
public class UpdateOrderDto
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public OrderStatus Status { get; set; }
}

/// <summary>
/// DTO for returning order data
/// </summary>
public class OrderDto
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public string StatusDescription { get; set; } = string.Empty;
    public int ProductCount { get; set; }
    public int TotalQuantity { get; set; }
    public List<OrderProductDetailDto> Products { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for product details in order
/// </summary>
public class OrderProductDetailDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalPrice { get; set; }
}

/// <summary>
/// Simplified DTO for listings
/// </summary>
public class OrderSummaryDto
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public OrderStatus Status { get; set; }
    public string StatusDescription { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
}

/// <summary>
/// DTO for adding product to an order
/// </summary>
public class AddProductToOrderDto
{
    [Required(ErrorMessage = "Order ID is required")]
    public Guid OrderId { get; set; }
    
    [Required(ErrorMessage = "Product ID is required")]
    public Guid ProductId { get; set; }
    
    [Required(ErrorMessage = "Quantity is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public int Quantity { get; set; }
    
    [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than 0")]
    public decimal? UnitPrice { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Discount cannot be negative")]
    public decimal Discount { get; set; } = 0;
}