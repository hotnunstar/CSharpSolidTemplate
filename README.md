# SOLID Template - Clean Architecture .NET 8 Web API

A comprehensive, production-ready template for building scalable web APIs using Clean Architecture principles and SOLID design patterns. This template serves as a foundation for enterprise-grade applications with built-in best practices, comprehensive documentation, and real-world examples.

## �️ Architecture Overview

```
├── Domain/          # 🏛️ Core business logic and entities
├── Application/     # 🎯 Use cases and business rules  
├── Infrastructure/  # 🔧 Data access and external services
├── Presentation/    # 🌐 API controllers and endpoints
└── docs/           # 📚 Comprehensive documentation
```

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK
- SQL Server (optional - uses In-Memory database by default)
- Visual Studio 2022 or VS Code

### Get Started
```bash
# Clone the template
git clone <repository-url>
cd SOLID_Template

# Restore dependencies
dotnet restore

# Run the application
dotnet run

# Access the API
# - Base URL: https://localhost:7299
# - Swagger UI: https://localhost:7299/swagger
# - Health Checks: https://localhost:7299/health
```

## 📚 Documentation

### 🎯 Core Guides
- **[Getting Started](docs/guides/getting-started.md)** - Complete setup and first steps
- **[Developer Guide](docs/guides/developer-guide.md)** - Complete development workflow with tips, tricks, and resources
- **[Clean Architecture](docs/architecture/clean-architecture.md)** - Architecture principles and structure
- **[Adding Features](docs/guides/adding-features.md)** - Step-by-step feature development
- **[SOLID Principles](docs/guides/solid-principles.md)** - SOLID principles and design patterns
- **[Testing Guide](docs/guides/testing.md)** - Comprehensive testing strategies

### 🛠️ Examples & Patterns
- **[Complete CRUD Example](docs/examples/crud-example.md)** - Full Task management implementation
- **[API Documentation](docs/api/)** - Endpoint specifications and usage
- **[Best Practices](docs/guides/solid-principles.md#-best-practices-summary)** - Development guidelines

### 📋 Reference
- **[Project Structure](docs/architecture/clean-architecture.md#-project-structure)** - Detailed folder organization
- **[Dependencies](docs/guides/getting-started.md#-dependencies)** - Package information
- **[Configuration](docs/guides/getting-started.md#-configuration)** - Environment setup

## ✨ Production Features

### 🎯 Core Features
- ✅ **Clean Architecture** - Separation of concerns with dependency inversion
- ✅ **SOLID Principles** - Maintainable and extensible code design
- ✅ **Entity Framework Core 8.0** - Code-first with migrations
- ✅ **AutoMapper** - Object-to-object mapping
- ✅ **FluentValidation** - Declarative validation rules
- ✅ **Swagger/OpenAPI** - Interactive API documentation

### 🛡️ Production-Ready
- ✅ **Global Exception Handling** - Centralized error management with ProblemDetails
- ✅ **Structured Logging** - Comprehensive logging with Serilog
- ✅ **Health Checks** - Application and database health monitoring
- ✅ **CORS Configuration** - Cross-origin resource sharing
- ✅ **Response Compression** - Performance optimization
- ✅ **Memory Caching** - In-memory caching foundation
- ✅ **Environment Configuration** - Development/Production settings

### 🧪 Quality Assurance
- ✅ **Unit Testing** - Comprehensive test coverage
- ✅ **Integration Testing** - End-to-end API testing
- ✅ **Validation Testing** - Input validation testing
- ✅ **Repository Testing** - Data access testing

## � Sample Implementation

### Person Management System
Complete CRUD operations demonstrating:

```http
GET    /api/persons           # Get all persons
GET    /api/persons/{id}      # Get person by ID
POST   /api/persons           # Create new person
PUT    /api/persons/{id}      # Update person
DELETE /api/persons/{id}      # Delete person (soft delete)
```

### Example Request/Response

```bash
# Create a new person
curl -X POST "https://localhost:7299/api/persons" \
     -H "Content-Type: application/json" \
     -d '{
       "firstName": "John",
       "lastName": "Doe", 
       "email": "john.doe@example.com",
       "dateOfBirth": "1990-01-15"
     }'
```

```json
{
  "success": true,
  "message": "Person created successfully",
  "data": {
    "id": 1,
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "fullName": "John Doe",
    "age": 34,
    "createdDate": "2024-01-15T10:30:00Z"
  }
}
```

## � Configuration

### Database Configuration
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SOLIDTemplateDB;Trusted_Connection=true;"
  },
  "UseInMemoryDatabase": false
}
```

### Logging Configuration
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft.AspNetCore": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "File", "Args": { "path": "logs/app-.txt", "rollingInterval": "Day" } }
    ]
  }
}
```

## 🛠️ Development Workflow

### Adding New Features

1. **📋 Plan** - Define requirements and design
2. **🏛️ Domain** - Create entities and business logic
3. **🔧 Infrastructure** - Implement data access
4. **🎯 Application** - Add services and DTOs
5. **🌐 Presentation** - Create API endpoints
6. **🧪 Test** - Write comprehensive tests
7. **📚 Document** - Update documentation

See [Adding Features Guide](docs/guides/adding-features.md) for detailed steps.

### Database Migrations
```bash
# Add new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

### Testing
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific category
dotnet test --filter Category=Unit
```

## 📊 Monitoring & Health

### Health Checks
- **Application Health**: `/health`
- **Database Health**: Automatic EF Core integration
- **Custom Health Checks**: Extensible health monitoring

### Logging & Monitoring
- **Structured Logging**: JSON-formatted logs with Serilog
- **Request Logging**: HTTP request/response logging
- **Error Tracking**: Centralized error handling and logging
- **Performance Metrics**: Built-in ASP.NET Core metrics

## 🏛️ Architecture Benefits

### 🎯 Maintainability
- **Clear Separation** of business logic, data access, and presentation
- **SOLID Principles** ensure code is easy to modify and extend
- **Dependency Injection** makes components loosely coupled and testable

### 🚀 Scalability  
- **Clean Architecture** allows independent scaling of layers
- **Repository Pattern** enables easy data store changes
- **Service Layer** provides centralized business logic

### 🧪 Testability
- **Dependency Injection** enables easy mocking
- **Interface-based Design** allows test doubles
- **Separated Concerns** enable focused unit tests

### 🔧 Flexibility
- **Plugin Architecture** through dependency injection
- **Strategy Patterns** for algorithmic flexibility  
- **Open/Closed Principle** for safe extensions

## � Tech Stack

### Core Framework
- **.NET 8.0** - Latest LTS framework with performance improvements
- **ASP.NET Core** - High-performance web API framework
- **Entity Framework Core 8.0** - Modern ORM with advanced features

### Libraries & Tools
- **AutoMapper 12.0** - Object-to-object mapping
- **FluentValidation 11.8** - Fluent validation rules
- **Serilog** - Structured logging framework
- **Swashbuckle** - OpenAPI/Swagger documentation
- **xUnit** - Unit testing framework
- **FluentAssertions** - Fluent assertion library

### Development Tools
- **Visual Studio 2022** - Full IDE support
- **VS Code** - Lightweight development
- **SQL Server** - Production database
- **In-Memory Database** - Development/testing

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

### Development Setup
1. Fork the repository
2. Create a feature branch: `git checkout -b feature/amazing-feature`
3. Make your changes and add tests
4. Ensure all tests pass: `dotnet test`
5. Commit your changes: `git commit -m 'Add amazing feature'`
6. Push to the branch: `git push origin feature/amazing-feature`
7. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **Clean Architecture** concepts by Robert C. Martin
- **SOLID Principles** by Robert C. Martin  
- **.NET Community** for excellent libraries and frameworks
- **Contributors** who help improve this template

---

**Ready to build amazing APIs?** Start with our [Getting Started Guide](docs/guides/getting-started.md) 🚀