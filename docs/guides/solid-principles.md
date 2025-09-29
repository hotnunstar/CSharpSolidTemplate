# SOLID Principles & Design Patterns

This guide explains how the SOLID principles and common design patterns are implemented throughout the template, providing practical examples and best practices.

## 🎯 SOLID Principles Overview

The **SOLID** principles are five design principles that make software designs more understandable, flexible, and maintainable:

- **S** - Single Responsibility Principle (SRP)
- **O** - Open/Closed Principle (OCP)
- **L** - Liskov Substitution Principle (LSP)
- **I** - Interface Segregation Principle (ISP)
- **D** - Dependency Inversion Principle (DIP)

## 1️⃣ Single Responsibility Principle (SRP)

> **A class should have only one reason to change.**

### ✅ Good Examples in the Template

#### Domain Entity

```csharp
// ✅ GOOD: Product entity has single responsibility - representing a product
public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Sku { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // Only product-related business logic
    public bool IsAvailable()
    {
        return IsActive && StockQuantity > 0;
    }

    public bool IsSkuValid()
    {
        return !string.IsNullOrEmpty(Sku) && Sku.Length >= 3;
    }

    public decimal CalculateDiscountPrice(decimal discountPercentage)
    {
        if (discountPercentage < 0 || discountPercentage > 100)
            throw new ArgumentOutOfRangeException(nameof(discountPercentage));
            
        return Price * (1 - discountPercentage / 100);
    }

    public bool TryReduceStock(int quantity)
    {
        if (quantity <= 0 || quantity > StockQuantity)
            return false;
            
        StockQuantity -= quantity;
        return true;
    }
}
```

#### Service Classes

```csharp
// ✅ GOOD: ProductService only handles product-related business operations
public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateProductDto> _validator;

    // Single responsibility: Product business logic coordination
    public async Task<ApiResponseDto<ProductDto>> CreateAsync(CreateProductDto dto)
    {
        // Validation
        var validationResult = await _validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return ApiResponseDto<ProductDto>.ErrorResult(errors);
        }

        var product = _mapper.Map<Product>(dto);
        var createdProduct = await _repository.AddAsync(product);
        var result = _mapper.Map<ProductDto>(createdProduct);
        return ApiResponseDto<ProductDto>.SuccessResult(result, "Product created successfully");
    }
}
```

### ❌ Anti-Pattern Examples

```csharp
// ❌ BAD: Multiple responsibilities in one class
public class ProductManager
{
    // Repository responsibility
    public void SaveProductToDatabase(Product product) { }
    
    // Notification service responsibility  
    public void SendStockAlert(Product product) { }
    
    // Logging responsibility
    public void LogProductActivity(string activity) { }
    
    // Validation responsibility
    public bool ValidateProductData(Product product) { }
    
    // Report generation responsibility
    public string GenerateProductReport(Product product) { }
}
```

### 🔧 How to Fix SRP Violations

```csharp
// ✅ GOOD: Separate classes for separate responsibilities

// 1. Repository for data access
public class ProductRepository : IProductRepository
{
    public async Task<Product> SaveAsync(Product product) { /* ... */ }
}

// 2. Notification service for alerts
public class NotificationService : INotificationService
{
    public async Task SendStockAlertAsync(string email, string productName) { /* ... */ }
}

// 3. Logger for logging
public class ProductLogger : ILogger<ProductService>
{
    public void LogProductActivity(string activity) { /* ... */ }
}

// 4. Validator for validation
public class CreateProductValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductValidator() { /* validation rules */ }
}

// 5. Report service for reports
public class ProductReportService : IProductReportService
{
    public string GenerateReport(IEnumerable<Product> products) { /* ... */ }
}
```

## 2️⃣ Open/Closed Principle (OCP)

> **Software entities should be open for extension but closed for modification.**

### ✅ Implementation in Template

#### Base Repository Pattern

```csharp
// ✅ GOOD: Base repository is closed for modification but open for extension
public abstract class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _context;

    protected BaseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Core functionality is closed for modification
    public virtual async Task<T> AddAsync(T entity)
    {
        _context.Set<T>().Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _context.Set<T>()
            .Where(e => !e.IsDeleted && e.Id == id)
            .FirstOrDefaultAsync();
    }
}

// ✅ GOOD: Extended for specific entity needs without modifying base
public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context) { }

    // Extension: Additional functionality specific to Product
    public async Task<Product?> GetBySkuAsync(string sku)
    {
        return await _context.Products
            .Where(p => !p.IsDeleted && p.Sku.ToUpper() == sku.ToUpper())
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Product>> GetAvailableProductsAsync()
    {
        return await _context.Products
            .Where(p => !p.IsDeleted && p.StockQuantity > 0)
            .ToListAsync()
            .ContinueWith(task => 
                task.Result.Where(p => p.IsAvailable()).ToList());
    }
}
```

#### Strategy Pattern for Extensibility

```csharp
// ✅ GOOD: Open for extension with new notification strategies
public interface INotificationStrategy
{
    Task SendAsync(string recipient, string subject, string message);
}

public class EmailNotificationStrategy : INotificationStrategy
{
    public async Task SendAsync(string recipient, string subject, string message)
    {
        // Email implementation
    }
}

public class SmsNotificationStrategy : INotificationStrategy
{
    public async Task SendAsync(string recipient, string subject, string message)
    {
        // SMS implementation
    }
}

// Easy to add new strategies without modifying existing code
public class SlackNotificationStrategy : INotificationStrategy
{
    public async Task SendAsync(string recipient, string subject, string message)
    {
        // Slack implementation
    }
}

public class NotificationService
{
    private readonly INotificationStrategy _strategy;

    public NotificationService(INotificationStrategy strategy)
    {
        _strategy = strategy;
    }

    public async Task SendNotificationAsync(string recipient, string subject, string message)
    {
        await _strategy.SendAsync(recipient, subject, message);
    }
}
```

### ❌ Anti-Pattern Example

```csharp
// ❌ BAD: Violates OCP - need to modify class for new notification types
public class NotificationService
{
    public async Task SendNotificationAsync(string type, string recipient, string message)
    {
        switch (type)
        {
            case "email":
                // Send email
                break;
            case "sms":
                // Send SMS
                break;
            // Need to modify this method to add new types!
            case "slack": // New requirement = modification!
                // Send Slack message
                break;
        }
    }
}
```

## 3️⃣ Liskov Substitution Principle (LSP)

> **Objects of a superclass should be replaceable with objects of its subclasses without altering the correctness of the program.**

### ✅ Good Implementation

```csharp
// ✅ GOOD: All repository implementations can substitute the base
public abstract class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity
{
    public virtual async Task<T> AddAsync(T entity)
    {
        // Base implementation
        entity.CreatedDate = DateTime.Now;
        _context.Set<T>().Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
}

public class ProductRepository : BaseRepository<Product>
{
    // ✅ GOOD: Honors the contract, just adds specific behavior
    public override async Task<Product> AddAsync(Product entity)
    {
        // Additional validation specific to Product
        if (await SkuExistsAsync(entity.Sku))
        {
            throw new ValidationException("SKU already exists");
        }
        
        // Call base implementation - maintains contract
        return await base.AddAsync(entity);
    }

    private async Task<bool> SkuExistsAsync(string sku) { /* ... */ }
}

public class OrderRepository : BaseRepository<Order>
{
    // ✅ GOOD: Also maintains the contract
    public override async Task<Order> AddAsync(Order entity)
    {
        // Additional business logic for orders
        entity.OrderNumber = await GenerateOrderNumberAsync();
        
        // Maintains the same contract
        return await base.AddAsync(entity);
    }
}
```

### ❌ LSP Violation Example

```csharp
// ❌ BAD: Violates LSP
public class ReadOnlyProductRepository : BaseRepository<Product>
{
    // ❌ BAD: Changes expected behavior - violates LSP
    public override async Task<Product> AddAsync(Product entity)
    {
        throw new NotSupportedException("This repository is read-only");
        // Clients expecting IBaseRepository<Product> will break!
    }
}

// ❌ BAD: Another LSP violation
public class AuditedProductRepository : BaseRepository<Product>
{
    // ❌ BAD: Changes the return type semantics
    public override async Task<Product> AddAsync(Product entity)
    {
        var result = await base.AddAsync(entity);
        // Returns null instead of the entity - violates contract expectation
        return null; 
    }
}
```

### 🔧 Correct LSP Implementation

```csharp
// ✅ GOOD: Create separate interfaces for different behaviors
public interface IReadOnlyRepository<T>
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
}

public interface IWriteRepository<T> : IReadOnlyRepository<T>
{
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}

// ✅ GOOD: ReadOnlyProductRepository implements appropriate interface
public class ReadOnlyProductRepository : IReadOnlyRepository<Product>
{
    public async Task<Product?> GetByIdAsync(int id) { /* ... */ }
    public async Task<IEnumerable<Product>> GetAllAsync() { /* ... */ }
    // No Add method - maintains contract
}
```

## 4️⃣ Interface Segregation Principle (ISP)

> **No client should be forced to depend on methods it does not use.**

### ✅ Good Implementation

```csharp
// ✅ GOOD: Small, focused interfaces
public interface IReadable<T>
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
}

public interface IWritable<T>
{
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
}

public interface IDeletable<T>
{
    Task DeleteAsync(T entity);
}

public interface ICacheable<T>
{
    Task<T?> GetFromCacheAsync(string key);
    Task SetCacheAsync(string key, T entity);
}

// ✅ GOOD: Compose interfaces as needed
public interface IFullRepository<T> : IReadable<T>, IWritable<T>, IDeletable<T>
{
}

public interface ICachedRepository<T> : IReadable<T>, ICacheable<T>
{
}

// ✅ GOOD: Implement only what's needed
public class ReadOnlyProductRepository : IReadable<Product>
{
    // Only implements read operations
    public async Task<Product?> GetByIdAsync(int id) { /* ... */ }
    public async Task<IEnumerable<Product>> GetAllAsync() { /* ... */ }
}

public class CachedProductRepository : ICachedRepository<Product>
{
    // Only implements read and cache operations
    public async Task<Product?> GetByIdAsync(int id) { /* ... */ }
    public async Task<IEnumerable<Product>> GetAllAsync() { /* ... */ }
    public async Task<Product?> GetFromCacheAsync(string key) { /* ... */ }
    public async Task SetCacheAsync(string key, Product entity) { /* ... */ }
}
```

### ❌ ISP Violation Example

```csharp
// ❌ BAD: Fat interface forces unnecessary implementations
public interface IRepository<T>
{
    // Read operations
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    
    // Write operations  
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    
    // Caching operations
    Task<T?> GetFromCacheAsync(string key);
    Task SetCacheAsync(string key, T entity);
    Task ClearCacheAsync();
    
    // Export operations
    Task<byte[]> ExportToCsvAsync();
    Task<byte[]> ExportToExcelAsync();
    
    // Audit operations
    Task<IEnumerable<AuditLog>> GetAuditLogsAsync(int entityId);
    Task LogAuditAsync(string action, T entity);
}

// ❌ BAD: Forced to implement methods not needed
public class SimpleProductRepository : IRepository<Product>
{
    public async Task<Product?> GetByIdAsync(int id) { /* ... */ }
    public async Task<IEnumerable<Product>> GetAllAsync() { /* ... */ }
    
    // ❌ Forced to implement caching even though not needed
    public async Task<Product?> GetFromCacheAsync(string key) 
    { 
        throw new NotImplementedException(); 
    }
    
    // ❌ Forced to implement export even though not needed
    public async Task<byte[]> ExportToCsvAsync() 
    { 
        throw new NotImplementedException(); 
    }
    
    // ... more unused implementations
}
```

## 5️⃣ Dependency Inversion Principle (DIP)

> **High-level modules should not depend on low-level modules. Both should depend on abstractions.**

### ✅ Good Implementation

```csharp
// ✅ GOOD: Service depends on abstraction, not concrete implementation
public class ProductService : IProductService
{
    // Depends on abstraction
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository repository,  // ← Abstraction
        IMapper mapper,                // ← Abstraction  
        ILogger<ProductService> logger) // ← Abstraction
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BaseResponseDto<ProductDto>> CreateAsync(CreateProductDto dto)
    {
        try
        {
            // Business logic doesn't know about concrete implementations
            var product = _mapper.Map<Product>(dto);
            var created = await _repository.AddAsync(product);
            var result = _mapper.Map<ProductDto>(created);
            
            _logger.LogInformation("Product created: {ProductId}", created.Id);
            return BaseResponseDto<ProductDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return BaseResponseDto<ProductDto>.Failure("Error creating product");
        }
    }
}

// ✅ GOOD: DI Registration properly configures dependencies
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register concrete implementations
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductService, ProductService>();
        
        return services;
    }
}
```

### ❌ DIP Violation Example

```csharp
// ❌ BAD: Direct dependency on concrete classes
public class ProductService
{
    private readonly ProductRepository _repository;      // ← Concrete class
    private readonly SqlServerLogger _logger;           // ← Concrete class
    private readonly ManualMapper _mapper;              // ← Concrete class

    public ProductService()
    {
        // ❌ BAD: Creates dependencies internally
        _repository = new ProductRepository(new SqlConnection("..."));
        _logger = new SqlServerLogger("connection string");
        _mapper = new ManualMapper();
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        // Tightly coupled to concrete implementations
        var product = _mapper.MapToEntity(dto);
        var created = await _repository.SaveToSqlServer(product);
        _logger.LogToSqlServer("Product created");
        
        return _mapper.MapToDto(created);
    }
}
```

## 🎨 Design Patterns in the Template

### 1. Repository Pattern

**Purpose:** Encapsulate data access logic and provide a more object-oriented view of the persistence layer.

```csharp
// Abstraction
public interface IProductRepository : IBaseRepository<Product>
{
    Task<Product?> GetBySkuAsync(string sku);
    Task<IEnumerable<Product>> GetAvailableProductsAsync();
}

// Implementation
public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Product?> GetBySkuAsync(string sku)
    {
        return await _context.Products
            .Where(p => !p.IsDeleted && p.Sku.ToUpper() == sku.ToUpper())
            .FirstOrDefaultAsync();
    }
}
```

**Benefits:**
- ✅ Separation of concerns
- ✅ Testability (easy to mock)
- ✅ Flexibility (can change persistence technology)
- ✅ Consistency across data access

### 2. Unit of Work Pattern

```csharp
public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }
    IOrderRepository Orders { get; }
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Products = new ProductRepository(_context);
        Orders = new OrderRepository(_context);
    }

    public IProductRepository Products { get; }
    public IOrderRepository Orders { get; }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
```

### 3. Strategy Pattern

**Purpose:** Define a family of algorithms, encapsulate each one, and make them interchangeable.

```csharp
// Strategy interface
public interface IPricingStrategy
{
    decimal CalculatePrice(Order order);
}

// Concrete strategies
public class StandardPricingStrategy : IPricingStrategy
{
    public decimal CalculatePrice(Order order)
    {
        return order.Items.Sum(i => i.Quantity * i.UnitPrice);
    }
}

public class BulkDiscountStrategy : IPricingStrategy
{
    public decimal CalculatePrice(Order order)
    {
        var total = order.Items.Sum(i => i.Quantity * i.UnitPrice);
        return total > 1000 ? total * 0.9m : total; // 10% discount for orders > $1000
    }
}

public class VipCustomerStrategy : IPricingStrategy
{
    public decimal CalculatePrice(Order order)
    {
        var total = order.Items.Sum(i => i.Quantity * i.UnitPrice);
        return total * 0.85m; // 15% discount for VIP customers
    }
}

// Context
public class OrderService
{
    private readonly IPricingStrategy _pricingStrategy;

    public OrderService(IPricingStrategy pricingStrategy)
    {
        _pricingStrategy = pricingStrategy;
    }

    public decimal CalculateOrderTotal(Order order)
    {
        return _pricingStrategy.CalculatePrice(order);
    }
}
```

### 4. Factory Pattern

**Purpose:** Create objects without specifying their concrete classes.

```csharp
public interface IReportFactory
{
    IReport CreateReport(ReportType type);
}

public class ReportFactory : IReportFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ReportFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IReport CreateReport(ReportType type)
    {
        return type switch
        {
            ReportType.Product => _serviceProvider.GetRequiredService<ProductReport>(),
            ReportType.Order => _serviceProvider.GetRequiredService<OrderReport>(),
            ReportType.Summary => _serviceProvider.GetRequiredService<SummaryReport>(),
            _ => throw new ArgumentException($"Unknown report type: {type}")
        };
    }
}
```

### 5. Builder Pattern

**Purpose:** Construct complex objects step by step.

```csharp
public class OrderBuilder
{
    private Order _order = new();

    public OrderBuilder WithId(int id)
    {
        _order.Id = id;
        return this;
    }

    public OrderBuilder WithCustomer(Person customer)
    {
        _order.Customer = customer;
        return this;
    }

    public OrderBuilder WithItem(string name, decimal price, int quantity)
    {
        _order.Items.Add(new OrderItem 
        { 
            Name = name, 
            UnitPrice = price, 
            Quantity = quantity 
        });
        return this;
    }

    public OrderBuilder WithStatus(OrderStatus status)
    {
        _order.Status = status;
        return this;
    }

    public OrderBuilder WithDate(DateTime date)
    {
        _order.OrderDate = date;
        return this;
    }

    public Order Build() => _order;

    public static OrderBuilder Create() => new();
}

// Usage
var order = OrderBuilder.Create()
    .WithCustomer(customer)
    .WithItem("Product A", 29.99m, 2)
    .WithItem("Product B", 19.99m, 1)
    .WithStatus(OrderStatus.Pending)
    .WithDate(DateTime.Now)
    .Build();
```

### 6. Decorator Pattern

**Purpose:** Add behavior to objects dynamically without altering their structure.

```csharp
// Base service
public class ProductService : IProductService
{
    public async Task<Product> CreateAsync(CreateProductDto dto)
    {
        // Core implementation
    }
}

// Decorators
public class CachedProductService : IProductService
{
    private readonly IProductService _innerService;
    private readonly IMemoryCache _cache;

    public CachedProductService(IProductService innerService, IMemoryCache cache)
    {
        _innerService = innerService;
        _cache = cache;
    }

    public async Task<Person> CreateAsync(CreatePersonDto dto)
    {
        var result = await _innerService.CreateAsync(dto);
        _cache.Set($"person_{result.Id}", result, TimeSpan.FromMinutes(30));
        return result;
    }
}

public class LoggedPersonService : IPersonService
{
    private readonly IPersonService _innerService;
    private readonly ILogger<LoggedPersonService> _logger;

    public LoggedPersonService(IPersonService innerService, ILogger<LoggedPersonService> logger)
    {
        _innerService = innerService;
        _logger = logger;
    }

    public async Task<Person> CreateAsync(CreatePersonDto dto)
    {
        _logger.LogInformation("Creating person with email: {Email}", dto.Email);
        
        try
        {
            var result = await _innerService.CreateAsync(dto);
            _logger.LogInformation("Person created successfully: {PersonId}", result.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating person with email: {Email}", dto.Email);
            throw;
        }
    }
}
```

### 7. Command Pattern

**Purpose:** Encapsulate requests as objects, allowing you to parameterize and queue operations.

```csharp
public interface ICommand<T>
{
    Task<T> ExecuteAsync();
}

public class CreatePersonCommand : ICommand<Person>
{
    private readonly CreatePersonDto _dto;
    private readonly IPersonRepository _repository;
    private readonly IMapper _mapper;

    public CreatePersonCommand(CreatePersonDto dto, IPersonRepository repository, IMapper mapper)
    {
        _dto = dto;
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Person> ExecuteAsync()
    {
        var person = _mapper.Map<Person>(_dto);
        return await _repository.AddAsync(person);
    }
}

public class CommandExecutor
{
    public async Task<T> ExecuteAsync<T>(ICommand<T> command)
    {
        // Can add cross-cutting concerns here
        return await command.ExecuteAsync();
    }
}
```

## 🎯 Best Practices Summary

### 1. **Dependency Injection**

```csharp
// ✅ DO: Use constructor injection
public class PersonService
{
    private readonly IPersonRepository _repository;
    
    public PersonService(IPersonRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }
}

// ❌ DON'T: Use service locator anti-pattern
public class PersonService
{
    public void DoSomething()
    {
        var repository = ServiceLocator.Get<IPersonRepository>(); // Anti-pattern
    }
}
```

### 2. **Interface Design**

```csharp
// ✅ DO: Small, focused interfaces
public interface IPersonReader
{
    Task<Person?> GetByIdAsync(int id);
}

public interface IPersonWriter
{
    Task<Person> AddAsync(Person person);
}

// ❌ DON'T: Large, monolithic interfaces
public interface IPersonEverything
{
    Task<Person> AddAsync(Person person);
    Task UpdateAsync(Person person);
    Task DeleteAsync(int id);
    Task<IEnumerable<Person>> SearchAsync(string term);
    Task<byte[]> ExportToPdfAsync();
    Task SendEmailAsync(int personId);
    // ... 20 more methods
}
```

### 3. **Error Handling**

```csharp
// ✅ DO: Use domain-specific exceptions
public class PersonNotFoundException : Exception
{
    public PersonNotFoundException(int personId) 
        : base($"Person with ID {personId} was not found") { }
}

// ✅ DO: Handle exceptions at appropriate levels
public class PersonService
{
    public async Task<BaseResponseDto<PersonDto>> GetByIdAsync(int id)
    {
        try
        {
            var person = await _repository.GetByIdAsync(id);
            if (person == null)
            {
                return BaseResponseDto<PersonDto>.Failure("Person not found");
            }
            
            var dto = _mapper.Map<PersonDto>(person);
            return BaseResponseDto<PersonDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving person with ID: {PersonId}", id);
            return BaseResponseDto<PersonDto>.Failure("An error occurred while retrieving the person");
        }
    }
}
```

### 4. **Testing Strategy**

- **Unit Tests**: Test individual components in isolation
- **Integration Tests**: Test component interactions
- **Mock External Dependencies**: Database, HTTP calls, file system
- **Test Business Logic**: Focus on domain rules and validations

This comprehensive guide demonstrates how SOLID principles and design patterns create maintainable, extensible, and testable code architecture.