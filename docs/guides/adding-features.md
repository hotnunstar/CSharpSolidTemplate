# Adding New Features Guide

This comprehensive guide walks you through adding new features to your SOLID Template project, following Clean Architecture principles and best practices.

## 🎯 Overview

Adding features follows a structured approach through all architecture layers:

1. **Domain Layer** - Define entities and business rules
2. **Infrastructure Layer** - Implement data access
3. **Application Layer** - Create business logic and DTOs
4. **Presentation Layer** - Add API endpoints
5. **Testing** - Create comprehensive tests

## 📋 Example: Adding a Product Feature

Let's add a complete Product management feature as an example.

### Requirements

- Create, read, update, delete products
- Products have name, description, price, category
- Business rule: Price must be positive
- Soft delete functionality

## Step 1: Domain Layer

### 1.1 Create Product Entity

**File:** `Domain/Entities/Product.cs`

```csharp
using SOLID_Template.Domain.Entities;

namespace SOLID_Template.Domain.Entities;

/// <summary>
/// Product entity representing a sellable item
/// </summary>
public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public int StockQuantity { get; set; }

    #region Business Logic

    /// <summary>
    /// Validates if the product price is acceptable
    /// </summary>
    public bool IsValidPrice()
    {
        return Price > 0;
    }

    /// <summary>
    /// Checks if product is in stock
    /// </summary>
    public bool IsInStock()
    {
        return StockQuantity > 0;
    }

    /// <summary>
    /// Reduces stock quantity (for sales)
    /// </summary>
    public bool TryReduceStock(int quantity)
    {
        if (StockQuantity >= quantity)
        {
            StockQuantity -= quantity;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Adds stock quantity (for restocking)
    /// </summary>
    public void AddStock(int quantity)
    {
        if (quantity > 0)
        {
            StockQuantity += quantity;
        }
    }

    #endregion
}
```

### 1.2 Create Repository Interface

**File:** `Domain/Interfaces/IProductRepository.cs`

```csharp
using SOLID_Template.Domain.Entities;

namespace SOLID_Template.Domain.Interfaces;

/// <summary>
/// Repository contract for Product operations
/// </summary>
public interface IProductRepository : IBaseRepository<Product>
{
    Task<IEnumerable<Product>> GetByCategoryAsync(string category);
    Task<IEnumerable<Product>> GetInStockAsync();
    Task<IEnumerable<Product>> SearchByNameAsync(string name);
    Task<Product?> GetByNameAsync(string name);
}
```

## Step 2: Infrastructure Layer

### 2.1 Implement Repository

**File:** `Infrastructure/Repositories/ProductRepository.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using SOLID_Template.Domain.Entities;
using SOLID_Template.Domain.Interfaces;
using SOLID_Template.Infrastructure.Data;

namespace SOLID_Template.Infrastructure.Repositories;

/// <summary>
/// Product repository implementation
/// </summary>
public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(string category)
    {
        return await _context.Products
            .Where(p => !p.IsDeleted && p.Category.ToLower() == category.ToLower())
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetInStockAsync()
    {
        return await _context.Products
            .Where(p => !p.IsDeleted && p.StockQuantity > 0)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> SearchByNameAsync(string name)
    {
        return await _context.Products
            .Where(p => !p.IsDeleted && p.Name.Contains(name))
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Product?> GetByNameAsync(string name)
    {
        return await _context.Products
            .Where(p => !p.IsDeleted && p.Name.ToLower() == name.ToLower())
            .FirstOrDefaultAsync();
    }
}
```

### 2.2 Update Database Context

**File:** `Infrastructure/Data/ApplicationDbContext.cs`

```csharp
// Add to the existing DbContext class
public DbSet<Product> Products { get; set; }

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Product configuration
    modelBuilder.Entity<Product>(entity =>
    {
        entity.Property(p => p.Name)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(p => p.Description)
            .HasMaxLength(500);

        entity.Property(p => p.Category)
            .HasMaxLength(50)
            .IsRequired();

        entity.Property(p => p.Price)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        entity.HasIndex(p => p.Name)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        entity.HasIndex(p => p.Category);
    });
}
```

## Step 3: Application Layer

### 3.1 Create DTOs

**File:** `Application/DTOs/ProductDtos.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace SOLID_Template.Application.DTOs;

/// <summary>
/// Product data transfer object for read operations
/// </summary>
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}

/// <summary>
/// DTO for creating new products
/// </summary>
public class CreateProductDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [Required]
    [StringLength(50)]
    public string Category { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }
}

/// <summary>
/// DTO for updating existing products
/// </summary>
public class UpdateProductDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [Required]
    [StringLength(50)]
    public string Category { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }
}

/// <summary>
/// DTO for updating product stock
/// </summary>
public class UpdateStockDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public int Quantity { get; set; }

    public string Operation { get; set; } = "add"; // "add" or "reduce"
}
```

### 3.2 Create Validators

**File:** `Application/Validators/CreateProductValidator.cs`

```csharp
using FluentValidation;
using SOLID_Template.Application.DTOs;
using SOLID_Template.Domain.Interfaces;

namespace SOLID_Template.Application.Validators;

/// <summary>
/// Validator for CreateProductDto
/// </summary>
public class CreateProductValidator : AbstractValidator<CreateProductDto>
{
    private readonly IProductRepository _productRepository;

    public CreateProductValidator(IProductRepository productRepository)
    {
        _productRepository = productRepository;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .Length(2, 100).WithMessage("Product name must be between 2 and 100 characters.")
            .MustAsync(BeUniqueProductName).WithMessage("A product with this name already exists.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.")
            .LessThanOrEqualTo(999999.99m).WithMessage("Price cannot exceed 999,999.99.");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required.")
            .Length(2, 50).WithMessage("Category must be between 2 and 50 characters.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.");
    }

    private async Task<bool> BeUniqueProductName(string name, CancellationToken token)
    {
        var existingProduct = await _productRepository.GetByNameAsync(name);
        return existingProduct == null;
    }
}
```

### 3.3 Create Service Interface

**File:** `Application/Interfaces/IProductService.cs`

```csharp
using SOLID_Template.Application.DTOs;

namespace SOLID_Template.Application.Interfaces;

/// <summary>
/// Product service contract
/// </summary>
public interface IProductService
{
    Task<BaseResponseDto<IEnumerable<ProductDto>>> GetAllAsync();
    Task<BaseResponseDto<ProductDto>> GetByIdAsync(int id);
    Task<BaseResponseDto<IEnumerable<ProductDto>>> GetByCategoryAsync(string category);
    Task<BaseResponseDto<IEnumerable<ProductDto>>> GetInStockAsync();
    Task<BaseResponseDto<IEnumerable<ProductDto>>> SearchByNameAsync(string name);
    Task<BaseResponseDto<ProductDto>> CreateAsync(CreateProductDto createProductDto);
    Task<BaseResponseDto<ProductDto>> UpdateAsync(int id, UpdateProductDto updateProductDto);
    Task<BaseResponseDto<bool>> UpdateStockAsync(int id, UpdateStockDto updateStockDto);
    Task<BaseResponseDto<bool>> DeleteAsync(int id);
}
```

### 3.4 Implement Service

**File:** `Application/Services/ProductService.cs`

```csharp
using AutoMapper;
using SOLID_Template.Application.DTOs;
using SOLID_Template.Application.Interfaces;
using SOLID_Template.Domain.Entities;
using SOLID_Template.Domain.Interfaces;

namespace SOLID_Template.Application.Services;

/// <summary>
/// Product service implementation
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public ProductService(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<BaseResponseDto<IEnumerable<ProductDto>>> GetAllAsync()
    {
        try
        {
            var products = await _productRepository.GetAllAsync();
            var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);
            return BaseResponseDto<IEnumerable<ProductDto>>.Success(productDtos);
        }
        catch (Exception ex)
        {
            return BaseResponseDto<IEnumerable<ProductDto>>.Failure($"Error retrieving products: {ex.Message}");
        }
    }

    public async Task<BaseResponseDto<ProductDto>> GetByIdAsync(int id)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return BaseResponseDto<ProductDto>.Failure("Product not found");
            }

            var productDto = _mapper.Map<ProductDto>(product);
            return BaseResponseDto<ProductDto>.Success(productDto);
        }
        catch (Exception ex)
        {
            return BaseResponseDto<ProductDto>.Failure($"Error retrieving product: {ex.Message}");
        }
    }

    public async Task<BaseResponseDto<ProductDto>> CreateAsync(CreateProductDto createProductDto)
    {
        try
        {
            var product = _mapper.Map<Product>(createProductDto);

            // Apply business rules
            if (!product.IsValidPrice())
            {
                return BaseResponseDto<ProductDto>.Failure("Invalid price. Price must be greater than 0.");
            }

            var createdProduct = await _productRepository.AddAsync(product);
            var productDto = _mapper.Map<ProductDto>(createdProduct);

            return BaseResponseDto<ProductDto>.Success(productDto, "Product created successfully");
        }
        catch (Exception ex)
        {
            return BaseResponseDto<ProductDto>.Failure($"Error creating product: {ex.Message}");
        }
    }

    public async Task<BaseResponseDto<bool>> UpdateStockAsync(int id, UpdateStockDto updateStockDto)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return BaseResponseDto<bool>.Failure("Product not found");
            }

            bool success;
            if (updateStockDto.Operation.ToLower() == "reduce")
            {
                success = product.TryReduceStock(updateStockDto.Quantity);
                if (!success)
                {
                    return BaseResponseDto<bool>.Failure("Insufficient stock quantity");
                }
            }
            else
            {
                product.AddStock(updateStockDto.Quantity);
                success = true;
            }

            await _productRepository.UpdateAsync(product);
            return BaseResponseDto<bool>.Success(success, "Stock updated successfully");
        }
        catch (Exception ex)
        {
            return BaseResponseDto<bool>.Failure($"Error updating stock: {ex.Message}");
        }
    }

    // ... implement other methods similarly
}
```

### 3.5 Update AutoMapper Profile

**File:** `Application/Mappings/MappingProfile.cs`

```csharp
// Add to existing MappingProfile class
CreateMap<Product, ProductDto>().ReverseMap();
CreateMap<CreateProductDto, Product>();
CreateMap<UpdateProductDto, Product>();
```

## Step 4: Presentation Layer

### 4.1 Create Controller

**File:** `Presentation/Controllers/ProductsController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using SOLID_Template.Application.DTOs;
using SOLID_Template.Application.Interfaces;

namespace SOLID_Template.Presentation.Controllers;

/// <summary>
/// Products API controller
/// </summary>
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
    public async Task<ActionResult<BaseResponseDto<IEnumerable<ProductDto>>>> GetAll()
    {
        var result = await _productService.GetAllAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<BaseResponseDto<ProductDto>>> GetById(int id)
    {
        var result = await _productService.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Get products by category
    /// </summary>
    [HttpGet("category/{category}")]
    public async Task<ActionResult<BaseResponseDto<IEnumerable<ProductDto>>>> GetByCategory(string category)
    {
        var result = await _productService.GetByCategoryAsync(category);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Create new product
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<BaseResponseDto<ProductDto>>> Create([FromBody] CreateProductDto createProductDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _productService.CreateAsync(createProductDto);
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result) : BadRequest(result);
    }

    /// <summary>
    /// Update product stock
    /// </summary>
    [HttpPatch("{id}/stock")]
    public async Task<ActionResult<BaseResponseDto<bool>>> UpdateStock(int id, [FromBody] UpdateStockDto updateStockDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _productService.UpdateStockAsync(id, updateStockDto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete product (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<BaseResponseDto<bool>>> Delete(int id)
    {
        var result = await _productService.DeleteAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
```

### 4.2 Update Dependency Injection

**File:** `Presentation/Extensions/ServiceCollectionExtensions.cs`

```csharp
// Add to AddApplicationServices method
services.AddScoped<IProductService, ProductService>();

// Add to AddRepositories method  
services.AddScoped<IProductRepository, ProductRepository>();

// Add to AddValidators method (FluentValidation will auto-discover)
// CreateProductValidator will be automatically registered
```

## Step 5: Testing

### 5.1 Domain Tests

**File:** `Tests/Domain/Entities/ProductTests.cs`

```csharp
using FluentAssertions;
using SOLID_Template.Domain.Entities;
using Xunit;

namespace SOLID_Template.Tests.Domain.Entities;

public class ProductTests
{
    [Fact]
    public void IsValidPrice_WhenPriceIsPositive_ReturnsTrue()
    {
        // Arrange
        var product = new Product { Price = 10.99m };

        // Act
        var result = product.IsValidPrice();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void TryReduceStock_WhenSufficientStock_ReducesStockAndReturnsTrue()
    {
        // Arrange
        var product = new Product { StockQuantity = 10 };

        // Act
        var result = product.TryReduceStock(5);

        // Assert
        result.Should().BeTrue();
        product.StockQuantity.Should().Be(5);
    }

    [Fact]
    public void TryReduceStock_WhenInsufficientStock_ReturnsFalse()
    {
        // Arrange
        var product = new Product { StockQuantity = 3 };

        // Act
        var result = product.TryReduceStock(5);

        // Assert
        result.Should().BeFalse();
        product.StockQuantity.Should().Be(3); // Unchanged
    }
}
```

### 5.2 Service Tests

**File:** `Tests/Application/Services/ProductServiceTests.cs`

```csharp
using AutoMapper;
using FluentAssertions;
using Moq;
using SOLID_Template.Application.DTOs;
using SOLID_Template.Application.Services;
using SOLID_Template.Domain.Entities;
using SOLID_Template.Domain.Interfaces;
using Xunit;

namespace SOLID_Template.Tests.Application.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _mockRepository = new Mock<IProductRepository>();
        _mockMapper = new Mock<IMapper>();
        _productService = new ProductService(_mockRepository.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductExists_ReturnsSuccessResponse()
    {
        // Arrange
        var productId = 1;
        var product = new Product { Id = productId, Name = "Test Product" };
        var productDto = new ProductDto { Id = productId, Name = "Test Product" };

        _mockRepository.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(product);
        _mockMapper.Setup(m => m.Map<ProductDto>(product))
            .Returns(productDto);

        // Act
        var result = await _productService.GetByIdAsync(productId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(productId);
    }

    [Fact]
    public async Task CreateAsync_WithValidProduct_ReturnsSuccessResponse()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "New Product",
            Price = 19.99m,
            Category = "Electronics"
        };

        var product = new Product
        {
            Name = createDto.Name,
            Price = createDto.Price,
            Category = createDto.Category
        };

        var createdProduct = new Product
        {
            Id = 1,
            Name = createDto.Name,
            Price = createDto.Price,
            Category = createDto.Category
        };

        var productDto = new ProductDto
        {
            Id = 1,
            Name = createDto.Name,
            Price = createDto.Price,
            Category = createDto.Category
        };

        _mockMapper.Setup(m => m.Map<Product>(createDto))
            .Returns(product);
        _mockRepository.Setup(r => r.AddAsync(product))
            .ReturnsAsync(createdProduct);
        _mockMapper.Setup(m => m.Map<ProductDto>(createdProduct))
            .Returns(productDto);

        // Act
        var result = await _productService.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Message.Should().Contain("created successfully");
    }
}
```

## Step 6: Database Migration

### 6.1 Create Migration

```bash
# Add Entity Framework tools if not installed
dotnet tool install --global dotnet-ef

# Create migration for Product entity
dotnet ef migrations add AddProductEntity --project YourProject.API

# Update database
dotnet ef database update --project YourProject.API
```

## ✅ Verification Checklist

After implementing the feature, verify:

- [ ] **Build Success:** Solution builds without errors
- [ ] **Tests Pass:** All unit tests pass
- [ ] **API Endpoints:** All endpoints respond correctly
- [ ] **Swagger Documentation:** Endpoints appear in Swagger UI
- [ ] **Database:** Migration applied successfully
- [ ] **Validation:** Input validation works as expected
- [ ] **Business Rules:** Domain logic is enforced
- [ ] **Error Handling:** Proper error responses
- [ ] **Logging:** Operations are logged appropriately

## 🚀 Next Steps

1. **Add Integration Tests:** Test the complete flow
2. **Add Authorization:** Secure the endpoints if needed  
3. **Add Caching:** Improve performance with caching
4. **Add Pagination:** For large data sets
5. **Add Filtering:** More advanced querying options

## 📚 Best Practices Summary

### ✅ DO's

- **Follow Layer Responsibilities:** Keep each layer focused
- **Use DTOs:** Never expose entities directly
- **Validate Input:** Use FluentValidation for robust validation  
- **Handle Errors:** Provide meaningful error messages
- **Write Tests:** Cover all business logic
- **Document APIs:** Use XML comments and Swagger

### ❌ DON'Ts

- **Don't Skip Layers:** Follow the architecture
- **Don't Put Business Logic in Controllers:** Keep controllers thin
- **Don't Ignore Validation:** Always validate external input
- **Don't Forget Tests:** Test-driven development is recommended
- **Don't Hardcode Values:** Use configuration and constants

This comprehensive approach ensures your new features are **maintainable**, **testable**, and follow the established **Clean Architecture** patterns.