# SOLID Template API

This is the Web API component of the SOLID Template solution, implementing Clean Architecture principles with a focus on maintainability, testability, and scalability.

## 🏗️ Architecture

The API follows Clean Architecture with the following layers:

### Domain Layer
Contains the core business entities and domain logic:
- **Entities**: Core business objects (`Person`, `Order`, `OrderPerson`)
- **Interfaces**: Repository contracts (`IPersonRepository`, `IOrderRepository`)
- **Enums**: Domain-specific enumerations (`OrderStatus`)

### Application Layer
Contains business logic and application services:
- **Services**: Business logic implementation (`PersonService`, `OrderService`)
- **Interfaces**: Service contracts (`IPersonService`, `IOrderService`)
- **DTOs**: Data transfer objects for external communication
- **Validators**: Input validation rules using FluentValidation
- **Mappings**: AutoMapper profiles for entity-DTO mapping

### Infrastructure Layer
Contains external concerns and data access:
- **Data**: Database context (`ApplicationDbContext`)
- **Repositories**: Data access implementation (`PersonRepository`, `OrderRepository`)

### Presentation Layer
Contains the Web API controllers and configuration:
- **Controllers**: API endpoints (`PersonsController`, `OrdersController`)
- **Extensions**: Service registration and configuration

## 📋 API Endpoints

### Persons Management
- `GET /api/persons` - Retrieve all persons
- `GET /api/persons/{id}` - Get person by ID
- `GET /api/persons/by-email/{email}` - Find person by email
- `GET /api/persons/search/{name}` - Search persons by name
- `POST /api/persons` - Create new person
- `PUT /api/persons/{id}` - Update existing person
- `DELETE /api/persons/{id}` - Soft delete person

### Orders Management
- `GET /api/orders` - Retrieve all orders
- `GET /api/orders/{id}` - Get order by ID
- `GET /api/orders/by-number/{number}` - Find order by number
- `GET /api/orders/by-status/{status}` - Filter orders by status
- `POST /api/orders` - Create new order
- `PUT /api/orders/{id}` - Update existing order
- `DELETE /api/orders/{id}` - Soft delete order

### Order-Person Relationships
- `POST /api/orders/add-person` - Associate person with order
- `DELETE /api/orders/{orderId}/persons/{personId}` - Remove person from order

### Order Status Management
- `PATCH /api/orders/{id}/approve` - Approve order (set status to Approved)
- `PATCH /api/orders/{id}/cancel` - Cancel order (set status to Cancelled)

## 🔧 Configuration

### Database Configuration
By default, the API uses Entity Framework Core with InMemory database for development:

```csharp
// In ServiceCollectionExtensions.cs
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("SOLID_TemplateDb"));
```

For production, you can switch to SQL Server or another provider:

```csharp
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
```

### Dependency Injection
All services are registered in `ServiceCollectionExtensions.cs`:

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase("SOLID_TemplateDb"));

        // Repositories
        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        // Services
        services.AddScoped<IPersonService, PersonService>();
        services.AddScoped<IOrderService, OrderService>();

        // AutoMapper
        services.AddAutoMapper(typeof(MappingProfile));

        // FluentValidation
        services.AddValidatorsFromAssemblyContaining<CreatePersonValidator>();

        return services;
    }
}
```

## 🎯 Key Features

### SOLID Principles Implementation
1. **Single Responsibility**: Each class has one reason to change
2. **Open/Closed**: Classes are open for extension, closed for modification
3. **Liskov Substitution**: Derived classes are substitutable for base classes
4. **Interface Segregation**: Clients depend only on interfaces they use
5. **Dependency Inversion**: High-level modules don't depend on low-level modules

### Repository Pattern
Data access is abstracted through repository interfaces:

```csharp
public interface IPersonRepository : IBaseRepository<Person>
{
    Task<Person?> GetByEmailAsync(string email);
    Task<IEnumerable<Person>> SearchByNameAsync(string name);
}
```

### Service Layer
Business logic is centralized in service classes:

```csharp
public interface IPersonService
{
    Task<BaseResponseDto<IEnumerable<PersonDto>>> GetAllAsync();
    Task<BaseResponseDto<PersonDto>> GetByIdAsync(int id);
    Task<BaseResponseDto<PersonDto>> CreateAsync(CreatePersonDto createPersonDto);
    // ... other methods
}
```

### Input Validation
FluentValidation ensures data integrity:

```csharp
public class CreatePersonValidator : AbstractValidator<CreatePersonDto>
{
    public CreatePersonValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
    }
}
```

### Object Mapping
AutoMapper handles entity-DTO conversions:

```csharp
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Person, PersonDto>().ReverseMap();
        CreateMap<CreatePersonDto, Person>();
        CreateMap<UpdatePersonDto, Person>();
        // ... other mappings
    }
}
```

## 🚀 Running the API

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code

### Development
1. Navigate to the API project directory
2. Run the application:
   ```powershell
   dotnet run
   ```
3. Access Swagger UI at: `https://localhost:5001/swagger` or `http://localhost:5000/swagger`

### Production Build
```powershell
dotnet build --configuration Release
dotnet publish --configuration Release
```

## 📊 Monitoring and Logging

The API includes built-in logging through .NET's ILogger interface. Logs are configured in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

## 🔒 Security Considerations

### Input Validation
- All DTOs are validated using FluentValidation
- Model binding validation prevents malformed requests
- SQL injection protection through Entity Framework parameterization

### Data Protection
- Soft delete implementation preserves data integrity
- Entity validation at domain level
- Repository pattern abstracts data access

## 📈 Performance Features

### Async/Await Pattern
All data operations use async/await for better scalability:

```csharp
public async Task<BaseResponseDto<PersonDto>> GetByIdAsync(int id)
{
    var person = await _personRepository.GetByIdAsync(id);
    // ... processing
}
```

### Efficient Querying
- Repository methods are optimized for specific use cases
- Entity Framework change tracking is properly managed
- InMemory database for fast development/testing

## 🔄 Extending the API

To add a new entity (e.g., `Product`):

1. **Create Domain Entity**:
   ```csharp
   // Domain/Entities/Product.cs
   public class Product : BaseEntity
   {
       public string Name { get; set; }
       public decimal Price { get; set; }
   }
   ```

2. **Create Repository Interface**:
   ```csharp
   // Domain/Interfaces/IProductRepository.cs
   public interface IProductRepository : IBaseRepository<Product>
   {
       Task<IEnumerable<Product>> GetByPriceRangeAsync(decimal min, decimal max);
   }
   ```

3. **Implement Repository**:
   ```csharp
   // Infrastructure/Repositories/ProductRepository.cs
   public class ProductRepository : BaseRepository<Product>, IProductRepository
   {
       // Implementation
   }
   ```

4. **Create DTOs**:
   ```csharp
   // Application/DTOs/ProductDtos.cs
   public class ProductDto
   {
       public int Id { get; set; }
       public string Name { get; set; }
       public decimal Price { get; set; }
   }
   ```

5. **Create Service**:
   ```csharp
   // Application/Services/ProductService.cs
   public class ProductService : IProductService
   {
       // Business logic implementation
   }
   ```

6. **Create Controller**:
   ```csharp
   // Presentation/Controllers/ProductsController.cs
   [ApiController]
   [Route("api/[controller]")]
   public class ProductsController : ControllerBase
   {
       // API endpoints
   }
   ```

7. **Register Services**:
   ```csharp
   // In ServiceCollectionExtensions.cs
   services.AddScoped<IProductRepository, ProductRepository>();
   services.AddScoped<IProductService, ProductService>();
   ```

## 📚 Additional Resources

- [ASP.NET Core Web API Documentation](https://docs.microsoft.com/aspnet/core/web-api/)
- [Entity Framework Core Guide](https://docs.microsoft.com/ef/core/)
- [Clean Architecture Principles](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [AutoMapper Documentation](https://automapper.org/)
- [FluentValidation Documentation](https://fluentvalidation.net/)