# Clean Architecture Overview

This document explains the Clean Architecture implementation in the SOLID Template and how it ensures maintainable, testable, and scalable code.

## 🎯 What is Clean Architecture?

Clean Architecture is a software design philosophy that emphasizes the **separation of concerns** through **layered architecture** where **dependencies point inward** toward the business logic.

### Key Principles

1. **Independence of Frameworks** - Business rules don't depend on external frameworks
2. **Testability** - Business rules can be tested without UI, Database, Web Server
3. **Independence of UI** - UI can change without changing business rules
4. **Independence of Database** - Business rules are not bound to the database
5. **Independence of External Agencies** - Business rules don't know about external interfaces

## 🏗️ Architecture Layers

The SOLID Template implements Clean Architecture with four distinct layers:

```
┌─────────────────────────────────────┐
│           Presentation              │ ← Controllers, Middleware, Extensions
├─────────────────────────────────────┤
│           Application               │ ← Services, DTOs, Validators, Mappings
├─────────────────────────────────────┤
│           Infrastructure            │ ← Repositories, Data Access, External APIs
├─────────────────────────────────────┤
│             Domain                  │ ← Entities, Business Rules, Interfaces
└─────────────────────────────────────┘
```

### Dependency Flow

```
Presentation ──→ Application ──→ Domain
      ↓              ↓
Infrastructure ─────→ Domain
```

**Key Rule:** Dependencies only point **inward**. Inner layers never depend on outer layers.

## 📁 Layer Breakdown

### 1. Domain Layer (Core)

**Location:** `Domain/`

**Purpose:** Contains the core business logic and entities.

**Responsibilities:**
- Business entities and value objects
- Domain interfaces (repository contracts)
- Business rules and domain logic
- Domain events and exceptions

**Dependencies:** None (Pure business logic)

```csharp
// Example: Domain Entity
public class Product : BaseEntity
{
    public string Name { get; set; }
    public string Sku { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }

    // Business logic methods
    public bool IsAvailable()
    {
        return StockQuantity > 0;
    }

    public bool IsSkuValid()
    {
        return !string.IsNullOrEmpty(Sku) && Sku.Length >= 3;
    }
}
```

### 2. Application Layer

**Location:** `Application/`

**Purpose:** Orchestrates business operations and use cases.

**Responsibilities:**
- Application services (use cases)
- DTOs (Data Transfer Objects)
- Input validation rules
- Object mapping configurations
- Application interfaces

**Dependencies:** Domain layer only

```csharp
// Example: Application Service
public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;

    public async Task<BaseResponseDto<ProductDto>> CreateAsync(CreateProductDto dto)
    {
        // 1. Validate input (handled by FluentValidation)
        // 2. Map DTO to entity
        var product = _mapper.Map<Product>(dto);
        
        // 3. Apply business rules
        if (!product.IsSkuValid())
            return BaseResponseDto<ProductDto>.Failure("Invalid SKU format");

        // 4. Save to repository
        await _repository.AddAsync(product);
        
        // 5. Return mapped result
        var result = _mapper.Map<ProductDto>(product);
        return BaseResponseDto<ProductDto>.Success(result);
    }
}
```

### 3. Infrastructure Layer

**Location:** `Infrastructure/`

**Purpose:** Implements external concerns and data access.

**Responsibilities:**
- Repository implementations
- Database context and configurations
- External API integrations
- File system access
- Email services, etc.

**Dependencies:** Domain and Application layers

```csharp
// Example: Repository Implementation
public class PersonRepository : BaseRepository<Person>, IPersonRepository
{
    public PersonRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Person?> GetByEmailAsync(string email)
    {
        return await _context.Persons
            .Where(p => !p.IsDeleted && p.Email == email)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Person>> SearchByNameAsync(string name)
    {
        return await _context.Persons
            .Where(p => !p.IsDeleted && p.Name.Contains(name))
            .ToListAsync();
    }
}
```

### 4. Presentation Layer

**Location:** `Presentation/`

**Purpose:** Handles HTTP requests and responses.

**Responsibilities:**
- API Controllers
- Middleware components
- Dependency injection configuration
- Request/response models
- API documentation

**Dependencies:** Application layer

```csharp
// Example: API Controller
[ApiController]
[Route("api/[controller]")]
public class PersonsController : ControllerBase
{
    private readonly IPersonService _personService;

    [HttpPost]
    public async Task<ActionResult<BaseResponseDto<PersonDto>>> Create(
        [FromBody] CreatePersonDto createPersonDto)
    {
        var result = await _personService.CreateAsync(createPersonDto);
        
        return result.Success 
            ? Ok(result) 
            : BadRequest(result);
    }
}
```

## 🔄 Data Flow Example

Let's trace a typical request through all layers:

### Scenario: Creating a New Person

```
1. HTTP POST /api/persons
   ↓
2. PersonsController.Create() [Presentation]
   ↓
3. PersonService.CreateAsync() [Application]
   ↓  
4. PersonRepository.AddAsync() [Infrastructure]
   ↓
5. ApplicationDbContext.SaveChanges() [Infrastructure]
   ↓
6. Response back through layers
```

### Step-by-Step Breakdown

```csharp
// 1. Presentation Layer - Controller receives HTTP request
[HttpPost]
public async Task<ActionResult> Create([FromBody] CreatePersonDto dto)
{
    // 2. Delegates to Application Service
    var result = await _personService.CreateAsync(dto);
    return Ok(result);
}

// 3. Application Layer - Service orchestrates the operation
public async Task<BaseResponseDto<PersonDto>> CreateAsync(CreatePersonDto dto)
{
    // Validation happens automatically via FluentValidation
    
    // Map DTO to Domain Entity
    var person = _mapper.Map<Person>(dto);
    
    // Apply business rules (Domain logic)
    if (!person.IsEmailValid())
        return BaseResponseDto<PersonDto>.Failure("Invalid email");
    
    // 4. Infrastructure Layer - Repository persists data
    var saved = await _repository.AddAsync(person);
    
    // Map back to DTO and return
    var result = _mapper.Map<PersonDto>(saved);
    return BaseResponseDto<PersonDto>.Success(result);
}

// 5. Infrastructure Layer - Repository implementation
public async Task<Person> AddAsync(Person entity)
{
    _context.Persons.Add(entity);
    await _context.SaveChangesAsync();
    return entity;
}
```

## 🎯 Benefits of This Architecture

### 1. Separation of Concerns

Each layer has a **single responsibility**:
- **Domain:** Business rules and entities
- **Application:** Use cases and orchestration  
- **Infrastructure:** Data access and external services
- **Presentation:** HTTP handling and API contracts

### 2. Testability

```csharp
// Easy to unit test - mock dependencies
[Test]
public async Task CreatePerson_ValidInput_ReturnsSuccess()
{
    // Arrange
    var mockRepository = new Mock<IPersonRepository>();
    var mockMapper = new Mock<IMapper>();
    var service = new PersonService(mockRepository.Object, mockMapper.Object);
    
    // Act & Assert
    var result = await service.CreateAsync(validDto);
    result.Success.Should().BeTrue();
}
```

### 3. Flexibility and Maintainability

- **Change Database:** Only Infrastructure layer changes
- **Change API Framework:** Only Presentation layer changes
- **Add Business Rules:** Only Domain/Application layers change
- **Add New Features:** Follow the same patterns

### 4. Dependency Inversion

```csharp
// Application depends on abstraction, not implementation
public class PersonService : IPersonService
{
    private readonly IPersonRepository _repository; // Interface, not concrete class
    
    // Infrastructure provides implementation
    services.AddScoped<IPersonRepository, PersonRepository>();
}
```

## 🔧 Implementation Guidelines

### DO's ✅

1. **Keep Domain Pure:** No external dependencies in Domain layer
2. **Use Interfaces:** Define contracts in Domain, implement in Infrastructure
3. **Map Between Layers:** Use DTOs for data transfer
4. **Follow Dependency Flow:** Dependencies always point inward
5. **Validate at Boundaries:** Validate input in Application layer

### DON'Ts ❌

1. **Don't Reference Outer Layers:** Inner layers should never reference outer layers
2. **Don't Put Business Logic in Controllers:** Keep controllers thin
3. **Don't Access Database from Domain:** Use repository pattern
4. **Don't Mix Concerns:** Each layer should have single responsibility
5. **Don't Skip Validation:** Always validate external input

## 📊 Architecture Benefits Summary

| Aspect | Traditional Layered | Clean Architecture |
|--------|-------------------|-------------------|
| **Testability** | Difficult (DB dependencies) | Easy (mockable interfaces) |
| **Flexibility** | Tight coupling | Loose coupling |
| **Maintainability** | Changes ripple through layers | Changes isolated to layers |
| **Business Logic** | Scattered across layers | Centralized in Domain/Application |
| **Dependencies** | Bidirectional | Unidirectional (inward) |

## 🚀 Extending the Architecture

### Adding a New Entity

1. **Domain Layer:** Create entity and interface
2. **Infrastructure Layer:** Implement repository
3. **Application Layer:** Create service, DTOs, validators
4. **Presentation Layer:** Add controller

### Adding External Services

1. **Domain Layer:** Define interface
2. **Infrastructure Layer:** Implement service
3. **Application Layer:** Use via dependency injection
4. **Presentation Layer:** Configure in DI container

## 📚 Additional Resources

- **[SOLID Principles](./solid-principles.md)** - Implementation details
- **[Design Patterns](./design-patterns.md)** - Patterns used in the template
- **[Testing Guide](../guides/testing-guide.md)** - Testing strategies for each layer
- **[Adding Features](../guides/adding-features.md)** - Practical implementation guide

## 🎯 Next Steps

1. **Study the Code:** Examine how entities, services, and repositories interact
2. **Follow Examples:** Look at Person and Order implementations
3. **Practice:** Try adding a new entity following the architecture
4. **Read Tests:** Unit tests show how to test each layer independently

The Clean Architecture ensures your codebase remains **maintainable**, **testable**, and **scalable** as your application grows.