# Complete CRUD Example

This example demonstrates how the SOLID Template implements a complete CRUD (Create, Read, Update, Delete) feature using Clean Architecture principles and best practices.

## 🎯 Scenario: Product Management System

We'll explore the **Product Management** feature that's implemented in this template, showing how it demonstrates complete CRUD operations with business logic in a real-world e-commerce context.

### Functional Requirements

- **Create** new products with name, SKU, price, description, category
- **Read** products (all, by ID, by SKU, by category, available products)  
- **Update** product details, pricing, and stock levels
- **Delete** products (soft delete)
- **Manage** product stock and availability
- **Integration** with orders through Product-Order relationships

### Business Rules

- Product name is required and should be descriptive
- SKU must be unique across all products
- Price must be greater than zero for active products
- Stock quantity cannot be negative
- Products can be activated/deactivated
- Low stock alerts when quantity falls below threshold
- Products can be associated with multiple orders

## 📋 Implementation Analysis

Let's analyze how the Product feature demonstrates Clean Architecture principles:

### Step 1: Domain Layer - Core Business Logic

#### 1.1 Product Entity

**File:** `Domain/Entities/Product.cs`

The Product entity contains rich business logic:

```csharp
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
```

**Key Business Logic Features:**

- **IsAvailable()**: Combines active status and stock availability
- **IsSkuValid()**: Validates SKU format according to business rules  
- **CalculateDiscountPrice()**: Handles pricing calculations with validation
- **TryReduceStock()**: Safely manages inventory reduction

#### 1.2 OrderProduct Junction Entity

**File:** `Domain/Entities/OrderProduct.cs`

```csharp
public class OrderProduct
{
    public Guid OrderId { get; set; }
    public virtual Order Order { get; set; } = null!;
    
    public Guid ProductId { get; set; }
    public virtual Product Product { get; set; } = null!;
    
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; } // Price at the time of order
    public decimal Discount { get; set; } = 0; // Discount applied to this product
    public DateTime AddedDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Calculates the total price for this product in the order
    /// </summary>
    public decimal GetTotalPrice()
    {
        return (Quantity * UnitPrice) - Discount;
    }
}
```

### Step 2: Infrastructure Layer - Data Access

#### 2.1 Product Repository

**File:** `Infrastructure/Repositories/ProductRepository.cs`

The repository implements specialized queries:

```csharp
public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    public async Task<Product?> GetBySkuAsync(string sku)
    {
        return await _context.Products
            .Where(p => !p.IsDeleted && p.Sku.ToLower() == sku.ToLower())
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(string category)
    {
        return await _context.Products
            .Where(p => !p.IsDeleted && 
                       p.Category.ToLower() == category.ToLower())
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetAvailableProductsAsync()
    {
        return await _context.Products
            .Where(p => !p.IsDeleted && 
                       p.IsActive && 
                       p.StockQuantity > 0)
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10)
    {
        return await _context.Products
            .Where(p => !p.IsDeleted && 
                       p.IsActive && 
                       p.StockQuantity <= threshold && 
                       p.StockQuantity > 0)
            .OrderBy(p => p.StockQuantity)
            .ThenBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<bool> IsSkuUniqueAsync(string sku, Guid? excludeProductId = null)
    {
        var query = _context.Products
            .Where(p => !p.IsDeleted && p.Sku.ToLower() == sku.ToLower());

        if (excludeProductId.HasValue)
        {
            query = query.Where(p => p.Id != excludeProductId.Value);
        }

        return !await query.AnyAsync();
    }
}
```

### Step 3: Application Layer - Business Services

#### 3.1 Product DTOs

**File:** `Application/DTOs/ProductDtos.cs`

```csharp
/// <summary>
/// DTO for creating new products
/// </summary>
public class CreateProductDto
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Sku { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
    public int StockQuantity { get; set; } = 0;

    [Required]
    [StringLength(100)]
    public string Category { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO for product responses
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
    public bool IsAvailable { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

### Step 4: Presentation Layer - API Controllers

#### 4.1 Products Controller

**File:** `Presentation/Controllers/ProductsController.cs`

The controller provides a clean REST API:

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Get all products
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _productService.GetAllAsync();
        return Ok(result);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _productService.GetByIdAsync(id);
        
        if (!result.Success)
            return NotFound(result);
            
        return Ok(result);
    }

    /// <summary>
    /// Create new product
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto createDto)
    {
        var result = await _productService.CreateAsync(createDto);
        
        if (!result.Success)
            return BadRequest(result);
            
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }
}
```

### Step 5: Integration with Orders

The template demonstrates how Products integrate with Orders:

```csharp
public class Order : BaseEntity
{
    public string Number { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    // Navigation Property
    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();

    /// <summary>
    /// Calculates the total amount of the order
    /// </summary>
    public decimal CalculateTotal()
    {
        return OrderProducts.Sum(op => op.GetTotalPrice());
    }
}
```

## 📊 API Usage Examples

### Creating a Product

```bash
POST /api/products
Content-Type: application/json

{
  "name": "Wireless Bluetooth Headphones",
  "description": "Premium noise-cancelling wireless headphones with 30-hour battery life",
  "price": 299.99,
  "sku": "WBH-001",
  "stockQuantity": 50,
  "category": "Electronics",
  "isActive": true
}
```

### Creating an Order with Products

```bash
POST /api/orders
Content-Type: application/json

{
  "number": "ORD-2025-001",
  "description": "Customer electronics order",
  "products": [
    {
      "productId": "123e4567-e89b-12d3-a456-426614174000",
      "quantity": 2,
      "unitPrice": 299.99,
      "discount": 20.00
    }
  ]
}
```

## ✅ Key Features Demonstrated

### 1. **Rich Domain Logic**

- Business rules embedded in entities (IsAvailable, TryReduceStock)
- Domain-driven design principles
- Calculation methods for business metrics (GetTotalPrice)

### 2. **Comprehensive Data Access**

- Specialized repository methods (GetByCategory, GetLowStock)  
- Efficient querying with indexes
- Unique constraint handling (SKU uniqueness)

### 3. **Robust Validation**

- FluentValidation with async validation
- Business rule enforcement at multiple levels
- Input sanitization and validation

### 4. **Complete CRUD Operations**

- Create, Read, Update, Delete operations
- Specialized update operations (stock management)
- Complex relationship management

### 5. **Production-Ready Features**

- Soft delete functionality
- Audit trail (created/updated dates)
- Error handling and logging
- Performance considerations with indexes

### 6. **Complex Relationships**

- Many-to-many relationship through OrderProduct
- Navigation properties for related data  
- Aggregate calculations across entities

## 🎯 Best Practices Demonstrated

- **Separation of Concerns:** Each layer has distinct responsibilities
- **Domain-Driven Design:** Rich domain models with business logic
- **SOLID Principles:** Dependency inversion, single responsibility, open/closed
- **API Design:** RESTful endpoints with proper HTTP verbs and status codes
- **Error Handling:** Consistent error responses with meaningful messages
- **Validation:** Input validation at multiple levels (DTO, FluentValidation, Domain)
- **Performance:** Indexed queries and efficient data access patterns
- **Testing:** Comprehensive unit tests for business logic  
- **Documentation:** Clear API documentation and code comments

## 🔗 Related Documentation

- [Architecture Overview](../architecture/clean-architecture.md)
- [SOLID Principles Guide](../guides/solid-principles.md)
- [Testing Strategies](../guides/testing.md)
- [Adding New Features](../guides/adding-features.md)

This Product-Order system provides a complete, production-ready implementation that demonstrates all aspects of Clean Architecture and SOLID principles in a real-world e-commerce business scenario.