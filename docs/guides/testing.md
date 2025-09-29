# Testing Guide

This guide provides comprehensive testing strategies and best practices for the SOLID Template, covering unit tests, integration tests, and testing patterns.

## 🎯 Testing Philosophy

Our testing approach follows the **Test Pyramid** principle:

```
       /\
      /  \     End-to-End Tests (Few)
     /____\
    /      \   Integration Tests (Some)  
   /________\
  /          \ Unit Tests (Many)
 /____________\
```

### Testing Principles

- **Fast Feedback:** Unit tests should run quickly
- **Isolation:** Tests should not depend on each other
- **Repeatability:** Tests should produce consistent results
- **Clear Intent:** Test names should describe what is being tested
- **ARRANGE-ACT-ASSERT:** Follow the AAA pattern consistently

## 🏗️ Test Project Structure

```
Tests/
├── SOLID_Template.Tests.Unit/
│   ├── Domain/
│   │   ├── Entities/
│   │   │   ├── ProductTests.cs
│   │   │   ├── OrderTests.cs
│   │   │   └── OrderProductTests.cs
│   │   └── Services/
│   ├── Application/
│   │   ├── Services/
│   │   │   ├── ProductServiceTests.cs
│   │   │   ├── OrderServiceTests.cs
│   │   │   └── MappingTests.cs
│   │   └── Validators/
│   │       ├── CreateProductValidatorTests.cs
│   │       └── CreateOrderValidatorTests.cs
│   └── Presentation/
│       └── Controllers/
│           ├── ProductsControllerTests.cs
│           └── OrdersControllerTests.cs
├── SOLID_Template.Tests.Integration/
│   ├── Controllers/
│   │   ├── ProductsControllerIntegrationTests.cs
│   │   └── OrdersControllerIntegrationTests.cs
│   ├── Repositories/
│   │   ├── ProductRepositoryTests.cs
│   │   └── OrderRepositoryTests.cs
│   └── Infrastructure/
│       └── TestWebApplicationFactory.cs
└── SOLID_Template.Tests.E2E/
    ├── Scenarios/
    │   ├── ProductManagementTests.cs
    │   └── OrderWorkflowTests.cs
    └── Support/
        └── TestDataBuilder.cs
```

## 🧪 Unit Testing

### 1. Domain Entity Testing

**File:** `Tests/Unit/Domain/Entities/ProductTests.cs`

```csharp
using FluentAssertions;
using SOLID_Template.Domain.Entities;
using Xunit;

namespace SOLID_Template.Tests.Unit.Domain.Entities;

public class ProductTests
{
    [Theory]
    [InlineData("PROD123", true)]
    [InlineData("ABC-123-XYZ", true)]
    [InlineData("123456", true)]
    [InlineData("", false)]
    [InlineData("AB", false)]
    public void IsSkuValid_ShouldReturnExpectedResult(string sku, bool expected)
    {
        // Arrange
        var product = new Product { Sku = sku };

        // Act
        var result = product.IsSkuValid();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(5, true)]
    [InlineData(100, true)]
    public void IsAvailable_ShouldCheckStockAvailability(int stock, bool expected)
    {
        // Arrange
        var product = new Product 
        { 
            StockQuantity = stock,
            IsActive = true 
        };

        // Act
        var result = product.IsAvailable();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(100.0, 10.0, 90.0)]
    [InlineData(50.0, 20.0, 40.0)]
    [InlineData(25.0, 0.0, 25.0)]
    public void CalculateDiscountPrice_ShouldApplyDiscountCorrectly(
        decimal price, decimal discountPercentage, decimal expected)
    {
        // Arrange
        var product = new Product 
        { 
            Price = price
        };

        // Act
        var discountedPrice = product.CalculateDiscountPrice(discountPercentage);

        // Assert
        discountedPrice.Should().Be(expected);
    }

    [Theory]
    [InlineData(10, 5, true, 5)]
    [InlineData(10, 10, true, 0)]
    [InlineData(5, 10, false, 5)]
    [InlineData(0, 1, false, 0)]
    public void TryReduceStock_ShouldHandleStockReduction(
        int initialStock, int quantity, bool expectedResult, int expectedStock)
    {
        // Arrange
        var product = new Product { StockQuantity = initialStock };

        // Act
        var result = product.TryReduceStock(quantity);

        // Assert
        result.Should().Be(expectedResult);
        product.StockQuantity.Should().Be(expectedStock);
    }

    [Fact]
    public void CalculateDiscountPrice_WhenInvalidDiscountPercentage_ShouldThrowException()
    {
        // Arrange
        var product = new Product { Price = 100m };

        // Act & Assert
        product.Invoking(p => p.CalculateDiscountPrice(-5))
            .Should().Throw<ArgumentOutOfRangeException>();
        
        product.Invoking(p => p.CalculateDiscountPrice(105))
            .Should().Throw<ArgumentOutOfRangeException>();

        // Act
        var isAdult = person.IsAdult();

        // Assert
        isAdult.Should().BeTrue();
    }
}
```

### 2. Service Layer Testing with Mocking

**File:** `Tests/Unit/Application/Services/PersonServiceTests.cs`

```csharp
using AutoMapper;
using FluentAssertions;
using Moq;
using SOLID_Template.Application.DTOs;
using SOLID_Template.Application.Services;
using SOLID_Template.Domain.Entities;
using SOLID_Template.Domain.Interfaces;
using Xunit;

namespace SOLID_Template.Tests.Unit.Application.Services;

public class PersonServiceTests
{
    private readonly Mock<IPersonRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly PersonService _service;

    public PersonServiceTests()
    {
        _mockRepository = new Mock<IPersonRepository>();
        _mockMapper = new Mock<IMapper>();
        _service = new PersonService(_mockRepository.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenPersonExists_ShouldReturnSuccess()
    {
        // Arrange
        var personId = 1;
        var person = new Person 
        { 
            Id = personId, 
            FirstName = "John", 
            LastName = "Doe",
            Email = "john@email.com"
        };
        var personDto = new PersonDto 
        { 
            Id = personId, 
            FirstName = "John", 
            LastName = "Doe",
            Email = "john@email.com"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(personId))
            .ReturnsAsync(person);
        _mockMapper.Setup(m => m.Map<PersonDto>(person))
            .Returns(personDto);

        // Act
        var result = await _service.GetByIdAsync(personId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(personDto);
        _mockRepository.Verify(r => r.GetByIdAsync(personId), Times.Once);
        _mockMapper.Verify(m => m.Map<PersonDto>(person), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenPersonNotFound_ShouldReturnFailure()
    {
        // Arrange
        var personId = 999;
        _mockRepository.Setup(r => r.GetByIdAsync(personId))
            .ReturnsAsync((Person)null);

        // Act
        var result = await _service.GetByIdAsync(personId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var createDto = new CreatePersonDto
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane@email.com",
            DateOfBirth = DateTime.Today.AddYears(-25)
        };

        var person = new Person
        {
            FirstName = "Jane",
            LastName = "Smith", 
            Email = "jane@email.com",
            DateOfBirth = DateTime.Today.AddYears(-25)
        };

        var createdPerson = new Person
        {
            Id = 1,
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane@email.com",
            DateOfBirth = DateTime.Today.AddYears(-25)
        };

        var resultDto = new PersonDto
        {
            Id = 1,
            FirstName = "Jane", 
            LastName = "Smith",
            Email = "jane@email.com"
        };

        _mockMapper.Setup(m => m.Map<Person>(createDto))
            .Returns(person);
        _mockRepository.Setup(r => r.AddAsync(person))
            .ReturnsAsync(createdPerson);
        _mockMapper.Setup(m => m.Map<PersonDto>(createdPerson))
            .Returns(resultDto);

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(resultDto);
        result.Message.Should().Contain("created successfully");
    }

    [Fact]
    public async Task CreateAsync_WhenRepositoryThrows_ShouldReturnFailure()
    {
        // Arrange
        var createDto = new CreatePersonDto
        {
            FirstName = "Jane",
            LastName = "Smith", 
            Email = "jane@email.com"
        };

        var person = new Person();
        _mockMapper.Setup(m => m.Map<Person>(createDto))
            .Returns(person);
        _mockRepository.Setup(r => r.AddAsync(person))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Database error");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_WhenCalled_ShouldReturnAllPersons()
    {
        // Arrange
        var persons = new List<Person>
        {
            new() { Id = 1, FirstName = "John", LastName = "Doe" },
            new() { Id = 2, FirstName = "Jane", LastName = "Smith" }
        };

        var personDtos = new List<PersonDto>
        {
            new() { Id = 1, FirstName = "John", LastName = "Doe" },
            new() { Id = 2, FirstName = "Jane", LastName = "Smith" }
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(persons);
        _mockMapper.Setup(m => m.Map<IEnumerable<PersonDto>>(persons))
            .Returns(personDtos);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Data.Should().BeEquivalentTo(personDtos);
    }
}
```

### 3. Controller Testing

**File:** `Tests/Unit/Presentation/Controllers/PersonsControllerTests.cs`

```csharp
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SOLID_Template.Application.DTOs;
using SOLID_Template.Application.Interfaces;
using SOLID_Template.Presentation.Controllers;
using Xunit;

namespace SOLID_Template.Tests.Unit.Presentation.Controllers;

public class PersonsControllerTests
{
    private readonly Mock<IPersonService> _mockService;
    private readonly PersonsController _controller;

    public PersonsControllerTests()
    {
        _mockService = new Mock<IPersonService>();
        _controller = new PersonsController(_mockService.Object);
    }

    [Fact]
    public async Task GetAll_WhenServiceReturnsSuccess_ShouldReturnOk()
    {
        // Arrange
        var personDtos = new List<PersonDto>
        {
            new() { Id = 1, FirstName = "John", LastName = "Doe" }
        };
        var response = BaseResponseDto<IEnumerable<PersonDto>>.Success(personDtos);

        _mockService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(response);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(response);
        _mockService.Verify(s => s.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAll_WhenServiceReturnsFailure_ShouldReturnBadRequest()
    {
        // Arrange
        var response = BaseResponseDto<IEnumerable<PersonDto>>.Failure("Error occurred");

        _mockService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(response);

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetById_WhenPersonExists_ShouldReturnOk()
    {
        // Arrange
        var personId = 1;
        var personDto = new PersonDto { Id = personId, FirstName = "John" };
        var response = BaseResponseDto<PersonDto>.Success(personDto);

        _mockService.Setup(s => s.GetByIdAsync(personId))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.GetById(personId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task GetById_WhenPersonNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var personId = 999;
        var response = BaseResponseDto<PersonDto>.Failure("Person not found");

        _mockService.Setup(s => s.GetByIdAsync(personId))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.GetById(personId);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Create_WithValidModel_ShouldReturnCreated()
    {
        // Arrange
        var createDto = new CreatePersonDto 
        { 
            FirstName = "John", 
            LastName = "Doe",
            Email = "john@email.com" 
        };
        var createdPersonDto = new PersonDto 
        { 
            Id = 1, 
            FirstName = "John", 
            LastName = "Doe" 
        };
        var response = BaseResponseDto<PersonDto>.Success(createdPersonDto, "Created successfully");

        _mockService.Setup(s => s.CreateAsync(createDto))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(PersonsController.GetById));
        createdResult.RouteValues["id"].Should().Be(1);
        createdResult.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task Create_WhenServiceFails_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = new CreatePersonDto 
        { 
            FirstName = "John", 
            LastName = "Doe" 
        };
        var response = BaseResponseDto<PersonDto>.Failure("Validation failed");

        _mockService.Setup(s => s.CreateAsync(createDto))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }
}
```

## 🔗 Integration Testing

### 1. Test Web Application Factory

**File:** `Tests/Integration/Infrastructure/TestWebApplicationFactory.cs`

```csharp
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SOLID_Template.Infrastructure.Data;

namespace SOLID_Template.Tests.Integration.Infrastructure;

public class TestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
            services.RemoveAll(typeof(ApplicationDbContext));

            // Add in-memory database for testing
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDatabase");
            });

            // Build service provider to create the database
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            // Ensure database is created
            dbContext.Database.EnsureCreated();
            
            // Seed test data if needed
            SeedTestData(dbContext);
        });

        builder.UseEnvironment("Testing");
    }

    private static void SeedTestData(ApplicationDbContext context)
    {
        // Add any test data seeding here
        if (!context.Persons.Any())
        {
            context.Persons.AddRange(
                new Person { FirstName = "John", LastName = "Doe", Email = "john@test.com" },
                new Person { FirstName = "Jane", LastName = "Smith", Email = "jane@test.com" }
            );
            context.SaveChanges();
        }
    }
}
```

### 2. Repository Integration Tests

**File:** `Tests/Integration/Repositories/PersonRepositoryTests.cs`

```csharp
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SOLID_Template.Domain.Entities;
using SOLID_Template.Infrastructure.Data;
using SOLID_Template.Infrastructure.Repositories;
using Xunit;

namespace SOLID_Template.Tests.Integration.Repositories;

public class PersonRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly PersonRepository _repository;

    public PersonRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new PersonRepository(_context);
    }

    [Fact]
    public async Task GetByEmailAsync_WhenPersonExists_ShouldReturnPerson()
    {
        // Arrange
        var person = new Person
        {
            FirstName = "John",
            LastName = "Doe", 
            Email = "john@test.com"
        };
        await _context.Persons.AddAsync(person);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync("john@test.com");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("john@test.com");
        result.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task GetByEmailAsync_WhenPersonNotExists_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByEmailAsync("nonexistent@test.com");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_WhenValidPerson_ShouldAddAndReturnPerson()
    {
        // Arrange
        var person = new Person
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane@test.com"
        };

        // Act
        var result = await _repository.AddAsync(person);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        
        var personInDb = await _context.Persons.FindAsync(result.Id);
        personInDb.Should().NotBeNull();
        personInDb!.Email.Should().Be("jane@test.com");
    }

    [Fact]
    public async Task DeleteAsync_WhenPersonExists_ShouldSoftDelete()
    {
        // Arrange
        var person = new Person
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com"
        };
        await _context.Persons.AddAsync(person);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(person);

        // Assert
        var deletedPerson = await _context.Persons.FindAsync(person.Id);
        deletedPerson.Should().NotBeNull();
        deletedPerson!.IsDeleted.Should().BeTrue();
        deletedPerson.DeletedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnOnlyNonDeletedPersons()
    {
        // Arrange
        var activePerson = new Person { FirstName = "Active", Email = "active@test.com" };
        var deletedPerson = new Person 
        { 
            FirstName = "Deleted", 
            Email = "deleted@test.com", 
            IsDeleted = true,
            DeletedDate = DateTime.Now
        };

        await _context.Persons.AddRangeAsync(activePerson, deletedPerson);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().FirstName.Should().Be("Active");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
```

### 3. Controller Integration Tests

**File:** `Tests/Integration/Controllers/PersonsControllerIntegrationTests.cs`

```csharp
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using SOLID_Template.Application.DTOs;
using SOLID_Template.Tests.Integration.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace SOLID_Template.Tests.Integration.Controllers;

public class PersonsControllerIntegrationTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public PersonsControllerIntegrationTests(TestWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllPersons()
    {
        // Act
        var response = await _client.GetAsync("/api/persons");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<BaseResponseDto<IEnumerable<PersonDto>>>(
            content, _jsonOptions);

        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetById_WhenPersonExists_ShouldReturnPerson()
    {
        // Arrange
        var personId = 1; // From seeded data

        // Act
        var response = await _client.GetAsync($"/api/persons/{personId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<BaseResponseDto<PersonDto>>(
            content, _jsonOptions);

        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(personId);
    }

    [Fact]
    public async Task GetById_WhenPersonNotExists_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = 9999;

        // Act
        var response = await _client.GetAsync($"/api/persons/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WithValidData_ShouldCreatePerson()
    {
        // Arrange
        var createDto = new CreatePersonDto
        {
            FirstName = "Integration",
            LastName = "Test",
            Email = "integration@test.com",
            DateOfBirth = DateTime.Today.AddYears(-30)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/persons", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<BaseResponseDto<PersonDto>>(
            content, _jsonOptions);

        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.FirstName.Should().Be("Integration");
        result.Data.Email.Should().Be("integration@test.com");

        // Verify location header
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task Create_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidDto = new CreatePersonDto
        {
            FirstName = "", // Invalid: empty name
            LastName = "Test",
            Email = "invalid-email" // Invalid: bad email format
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/persons", invalidDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WithValidData_ShouldUpdatePerson()
    {
        // Arrange
        var personId = 1;
        var updateDto = new UpdatePersonDto
        {
            FirstName = "Updated",
            LastName = "Name", 
            Email = "updated@test.com",
            DateOfBirth = DateTime.Today.AddYears(-25)
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/persons/{personId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<BaseResponseDto<PersonDto>>(
            content, _jsonOptions);

        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.FirstName.Should().Be("Updated");
        result.Data.LastName.Should().Be("Name");
    }

    [Fact]
    public async Task Delete_WhenPersonExists_ShouldDeletePerson()
    {
        // First create a person to delete
        var createDto = new CreatePersonDto
        {
            FirstName = "ToDelete",
            LastName = "Person",
            Email = "delete@test.com"
        };
        
        var createResponse = await _client.PostAsJsonAsync("/api/persons", createDto);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createResult = JsonSerializer.Deserialize<BaseResponseDto<PersonDto>>(
            createContent, _jsonOptions);
        
        var personId = createResult!.Data!.Id;

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/persons/{personId}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify person is no longer accessible
        var getResponse = await _client.GetAsync($"/api/persons/{personId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
```

## 🎭 End-to-End Testing

### Scenario-Based E2E Tests

**File:** `Tests/E2E/Scenarios/PersonManagementTests.cs`

```csharp
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using SOLID_Template.Application.DTOs;
using SOLID_Template.Tests.Integration.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace SOLID_Template.Tests.E2E.Scenarios;

public class PersonManagementTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public PersonManagementTests(TestWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    [Fact]
    public async Task CompletePersonLifecycle_ShouldWorkEndToEnd()
    {
        // 1. Create a new person
        var createDto = new CreatePersonDto
        {
            FirstName = "E2E",
            LastName = "Test",
            Email = "e2e@test.com",
            DateOfBirth = DateTime.Today.AddYears(-28)
        };

        var createResponse = await _client.PostAsJsonAsync("/api/persons", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createResult = JsonSerializer.Deserialize<BaseResponseDto<PersonDto>>(
            createContent, _jsonOptions);
        
        var personId = createResult!.Data!.Id;

        // 2. Retrieve the created person
        var getResponse = await _client.GetAsync($"/api/persons/{personId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getContent = await getResponse.Content.ReadAsStringAsync();
        var getResult = JsonSerializer.Deserialize<BaseResponseDto<PersonDto>>(
            getContent, _jsonOptions);

        getResult!.Data!.FirstName.Should().Be("E2E");
        getResult.Data.Email.Should().Be("e2e@test.com");

        // 3. Update the person
        var updateDto = new UpdatePersonDto
        {
            FirstName = "Updated E2E",
            LastName = "Updated Test",
            Email = "updated-e2e@test.com",
            DateOfBirth = DateTime.Today.AddYears(-30)
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/persons/{personId}", updateDto);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 4. Verify the update
        var getUpdatedResponse = await _client.GetAsync($"/api/persons/{personId}");
        var getUpdatedContent = await getUpdatedResponse.Content.ReadAsStringAsync();
        var getUpdatedResult = JsonSerializer.Deserialize<BaseResponseDto<PersonDto>>(
            getUpdatedContent, _jsonOptions);

        getUpdatedResult!.Data!.FirstName.Should().Be("Updated E2E");
        getUpdatedResult.Data.Email.Should().Be("updated-e2e@test.com");

        // 5. Get all persons and verify our person is included
        var getAllResponse = await _client.GetAsync("/api/persons");
        var getAllContent = await getAllResponse.Content.ReadAsStringAsync();
        var getAllResult = JsonSerializer.Deserialize<BaseResponseDto<IEnumerable<PersonDto>>>(
            getAllContent, _jsonOptions);

        getAllResult!.Data.Should().Contain(p => p.Id == personId);

        // 6. Delete the person
        var deleteResponse = await _client.DeleteAsync($"/api/persons/{personId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 7. Verify deletion
        var getFinalResponse = await _client.GetAsync($"/api/persons/{personId}");
        getFinalResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DuplicateEmailValidation_ShouldPreventDuplicates()
    {
        var email = "duplicate@test.com";

        // 1. Create first person
        var firstPersonDto = new CreatePersonDto
        {
            FirstName = "First",
            LastName = "Person",
            Email = email
        };

        var firstResponse = await _client.PostAsJsonAsync("/api/persons", firstPersonDto);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // 2. Attempt to create second person with same email
        var secondPersonDto = new CreatePersonDto
        {
            FirstName = "Second",
            LastName = "Person", 
            Email = email
        };

        var secondResponse = await _client.PostAsJsonAsync("/api/persons", secondPersonDto);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
```

## 🧰 Testing Utilities

### Test Data Builders

**File:** `Tests/Support/TestDataBuilder.cs`

```csharp
using SOLID_Template.Application.DTOs;
using SOLID_Template.Domain.Entities;

namespace SOLID_Template.Tests.Support;

/// <summary>
/// Builder pattern for creating test data
/// </summary>
public class PersonTestDataBuilder
{
    private Person _person = new();

    public PersonTestDataBuilder WithId(int id)
    {
        _person.Id = id;
        return this;
    }

    public PersonTestDataBuilder WithName(string firstName, string lastName)
    {
        _person.FirstName = firstName;
        _person.LastName = lastName;
        return this;
    }

    public PersonTestDataBuilder WithEmail(string email)
    {
        _person.Email = email;
        return this;
    }

    public PersonTestDataBuilder WithAge(int age)
    {
        _person.DateOfBirth = DateTime.Today.AddYears(-age);
        return this;
    }

    public PersonTestDataBuilder AsDeleted()
    {
        _person.IsDeleted = true;
        _person.DeletedDate = DateTime.Now;
        return this;
    }

    public Person Build() => _person;

    public static PersonTestDataBuilder Default() => new PersonTestDataBuilder()
        .WithName("John", "Doe")
        .WithEmail("john@test.com")
        .WithAge(30);
}

/// <summary>
/// Builder for CreatePersonDto
/// </summary>
public class CreatePersonDtoBuilder
{
    private CreatePersonDto _dto = new();

    public CreatePersonDtoBuilder WithName(string firstName, string lastName)
    {
        _dto.FirstName = firstName;
        _dto.LastName = lastName;
        return this;
    }

    public CreatePersonDtoBuilder WithEmail(string email)
    {
        _dto.Email = email;
        return this;
    }

    public CreatePersonDtoBuilder WithDateOfBirth(DateTime dateOfBirth)
    {
        _dto.DateOfBirth = dateOfBirth;
        return this;
    }

    public CreatePersonDto Build() => _dto;

    public static CreatePersonDtoBuilder Default() => new CreatePersonDtoBuilder()
        .WithName("Test", "User")
        .WithEmail("test@example.com")
        .WithDateOfBirth(DateTime.Today.AddYears(-25));
}
```

## 🏃‍♂️ Test Execution

### Running Tests

```bash
# Run all tests
dotnet test

# Run unit tests only  
dotnet test --filter Category=Unit

# Run integration tests only
dotnet test --filter Category=Integration

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter PersonServiceTests

# Run tests with detailed output
dotnet test --verbosity detailed
```

### Test Categories

```csharp
[Fact, Trait("Category", "Unit")]
public void UnitTest_Example() { }

[Fact, Trait("Category", "Integration")]  
public void IntegrationTest_Example() { }

[Fact, Trait("Category", "E2E")]
public void E2ETest_Example() { }
```

## 📊 Test Coverage

### Coverage Configuration

**File:** `Tests/coverlet.runsettings`

```xml
<?xml version="1.0" encoding="utf-8" ?>
<RunSettings>
  <DataCollectionRunSettings>
    <DataCollectors>
      <DataCollector friendlyName="XPlat code coverage">
        <Configuration>
          <Format>json,cobertura,lcov,teamcity,opencover</Format>
          <Exclude>[*.Tests*]*</Exclude>
          <ExcludeByAttribute>Obsolete,GeneratedCodeAttribute,CompilerGeneratedAttribute</ExcludeByAttribute>
        </Configuration>
      </DataCollector>
    </DataCollectors>
  </DataCollectionRunSettings>
</RunSettings>
```

### Coverage Reports

```bash
# Generate coverage report
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings

# Generate HTML report (requires reportgenerator tool)
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage-report -reporttypes:Html
```

## ✅ Testing Best Practices

### 1. **Test Naming Conventions**

```csharp
// Pattern: MethodName_Scenario_ExpectedBehavior
public void GetById_WhenPersonExists_ShouldReturnPerson()
public void Create_WithInvalidEmail_ShouldThrowValidationException()
public void CalculateAge_WhenDateOfBirthIsToday_ShouldReturnZero()
```

### 2. **AAA Pattern**

```csharp
[Fact]
public void Example_Test()
{
    // Arrange - Set up test data and dependencies
    var person = new Person { Name = "Test" };
    var mockRepo = new Mock<IRepository>();
    
    // Act - Execute the method under test
    var result = service.ProcessPerson(person);
    
    // Assert - Verify the expected outcome
    result.Should().NotBeNull();
    mockRepo.Verify(x => x.Save(person), Times.Once);
}
```

### 3. **Test Data Management**

- Use **Object Mothers** or **Test Data Builders** for complex objects
- Keep test data **minimal and focused**
- Use **realistic but not real** data
- Avoid **magic numbers and strings**

### 4. **Mocking Guidelines**

- Mock **external dependencies** only
- Don't mock the **system under test**
- Use **strict mocks** to catch unexpected calls
- Verify **behavior, not implementation**

### 5. **Test Organization**

- **One assertion per test** (when possible)
- **Independent tests** that can run in any order
- **Descriptive test names** that explain the scenario
- **Group related tests** using nested classes

## 🔄 Continuous Testing

### GitHub Actions Workflow

**File:** `.github/workflows/test.yml`

```yaml
name: Tests

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  test:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
        
    - name: Restore dependencies
      run: dotnet restore
      
    - name: Build
      run: dotnet build --no-restore
      
    - name: Run unit tests
      run: dotnet test --no-build --filter Category=Unit --logger trx --collect:"XPlat Code Coverage"
      
    - name: Run integration tests
      run: dotnet test --no-build --filter Category=Integration --logger trx
      
    - name: Upload coverage reports
      uses: codecov/codecov-action@v3
      with:
        files: '**/coverage.cobertura.xml'
```

This comprehensive testing guide provides the foundation for maintaining high-quality, reliable code throughout the development lifecycle.