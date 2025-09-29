# SOLID Template - Unit Tests

This document describes the unit testing structure implemented for the SOLID Template project following Test-Driven Development (TDD) and SOLID principles.

## 🧪 Testing Architecture

The test project follows the same architectural layers as the main project, ensuring consistency and maintainability:

### 📁 Test Structure

```
SOLID_Template.Tests/
├── Application/        # Application layer tests
│   └── Services/       # Service tests with mocked dependencies
├── Domain/             # Domain layer tests
│   └── Entities/       # Entity business logic tests
├── Infrastructure/     # Infrastructure layer tests
│   └── Repositories/   # Repository tests with in-memory database
├── Presentation/       # Presentation layer tests
│   └── Controllers/    # Controller tests with mocked services
└── Helpers/            # Test helper classes and utilities
```

## 🎯 Testing Principles (Following SOLID)

### **S** - Single Responsibility Principle
- Each test class tests only one specific class or component
- Each test method tests only one specific behavior
- Helper classes have single, focused responsibilities

### **O** - Open/Closed Principle
- Test base classes are open for extension
- New test scenarios can be added without modifying existing tests

### **L** - Liskov Substitution Principle
- Mocked dependencies can replace real implementations seamlessly
- Test doubles follow the same contracts as real objects

### **I** - Interface Segregation Principle
- Tests depend only on the interfaces they need
- Mocked interfaces are specific and focused

### **D** - Dependency Inversion Principle
- Tests depend on abstractions (interfaces) through mocking
- High-level test logic doesn't depend on concrete implementations

## 🛠️ Testing Technologies

- **xUnit**: Main testing framework
- **Moq**: Mocking framework for dependency isolation
- **FluentAssertions**: Expressive assertion library
- **Entity Framework InMemory**: In-memory database for integration tests
- **AutoMapper**: Object mapping (same configuration as main project)

## 📋 Test Categories

### 1. Unit Tests (Domain Layer)
**Location**: `Domain/Entities/`
**Purpose**: Test business logic and domain rules
**Dependencies**: None (pure unit tests)

```csharp
[Fact]
public void CalculateAge_Should_Return_Correct_Age()
{
    // Arrange
    var person = new Person { BirthDate = new DateTime(1990, 1, 15) };
    
    // Act
    var age = person.CalculateAge();
    
    // Assert
    age.Should().Be(35); // Assuming current year is 2025
}
```

### 2. Service Tests (Application Layer)
**Location**: `Application/Services/`
**Purpose**: Test service layer business logic
**Dependencies**: Mocked repositories and external services

```csharp
[Fact]
public async Task GetByIdAsync_Should_Return_Success_When_Person_Exists()
{
    // Arrange
    var personId = Guid.NewGuid();
    _personRepositoryMock
        .Setup(r => r.GetByIdAsync(personId))
        .ReturnsAsync(person);

    // Act
    var result = await _personService.GetByIdAsync(personId);

    // Assert
    result.Success.Should().BeTrue();
    result.Data.Should().NotBeNull();
}
```

### 3. Repository Tests (Infrastructure Layer)
**Location**: `Infrastructure/Repositories/`
**Purpose**: Test data access layer
**Dependencies**: In-memory database context

```csharp
[Fact]
public async Task GetByIdAsync_Should_Return_Person_When_Exists()
{
    await TestDbContextHelper.WithCleanContextAsync(async context =>
    {
        var repository = new PersonRepository(context);
        // Test implementation
    });
}
```

### 4. Controller Tests (Presentation Layer)
**Location**: `Presentation/Controllers/`
**Purpose**: Test HTTP endpoints and request/response handling
**Dependencies**: Mocked services

```csharp
[Fact]
public async Task GetById_Should_Return_Ok_When_Person_Exists()
{
    // Arrange
    _personServiceMock
        .Setup(s => s.GetByIdAsync(personId))
        .ReturnsAsync(successResponse);

    // Act
    var result = await _controller.GetById(personId);

    // Assert
    result.Should().BeOfType<OkObjectResult>();
}
```

## 🔧 Test Helpers

### TestDbContextHelper
Provides utilities for creating and managing test database contexts:

- `CreateInMemoryContext()`: Creates isolated in-memory database
- `WithCleanContext()`: Ensures cleanup after test execution
- `WithCleanContextAsync()`: Async version for async operations

## 🚀 Running Tests

### Run All Tests
```bash
dotnet test
```

### Run Tests with Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Run Specific Test Category
```bash
# Run only Domain tests
dotnet test --filter "FullyQualifiedName~Domain"

# Run only Service tests  
dotnet test --filter "FullyQualifiedName~Services"
```

### Verbose Output
```bash
dotnet test --verbosity normal
```

## 📊 Test Coverage Goals

- **Domain Layer**: 90%+ coverage (critical business logic)
- **Application Layer**: 80%+ coverage (service orchestration)
- **Infrastructure Layer**: 70%+ coverage (data access)
- **Presentation Layer**: 70%+ coverage (API contracts)

## 🧪 Best Practices Implemented

### 1. **AAA Pattern** (Arrange, Act, Assert)
All tests follow the clear AAA structure for readability.

### 2. **Descriptive Test Names**
Test method names clearly describe what is being tested and expected outcome.

### 3. **Isolated Tests**
Each test is independent and can run in any order.

### 4. **Mocking Strategy**
- Mock external dependencies
- Use real objects for value objects and DTOs
- Prefer behavior verification over state verification

### 5. **Data Management**
- Use separate in-memory database per test
- Clean up resources after each test
- Use deterministic test data

### 6. **Async Testing**
Proper handling of async/await patterns in tests.

## 🔄 Continuous Integration

Tests are designed to run in CI/CD pipelines:

- Fast execution (under 30 seconds)
- No external dependencies
- Deterministic results
- Clear failure messages

## 📚 Testing Anti-Patterns Avoided

- ❌ Testing implementation details instead of behavior
- ❌ Brittle tests that break with refactoring
- ❌ Tests that depend on external systems
- ❌ Overly complex test setup
- ❌ Testing multiple behaviors in one test
- ❌ Shared state between tests

## 🔍 Test Maintenance

### Adding New Tests
1. Follow existing naming conventions
2. Use appropriate test category structure
3. Include both positive and negative scenarios
4. Mock dependencies appropriately
5. Ensure tests are isolated and deterministic

### Refactoring Tests
1. Keep tests updated when implementation changes
2. Maintain test readability and simplicity
3. Update mocks when interfaces change
4. Ensure coverage is maintained or improved

This testing approach ensures the SOLID Template maintains high quality, reliability, and maintainability while following industry best practices.