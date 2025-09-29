# Getting Started Guide

This guide will help you set up and start using the SOLID Template for your new Web API projects.

## 🎯 Prerequisites

Before starting, ensure you have the following installed:

### Required Software

- **.NET 8.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Git** - [Download here](https://git-scm.com/)

### Recommended Tools

- **Visual Studio 2022** (Community, Professional, or Enterprise)
- **Visual Studio Code** with C# extension
- **Postman** or **Insomnia** for API testing
- **SQL Server Management Studio** (for production database)

### Verify Installation

```bash
# Check .NET version
dotnet --version
# Should show 8.0.x or higher

# Check Git installation  
git --version
```

## 🚀 Quick Setup

### Step 1: Clone the Template

```bash
# Clone the repository
git clone [your-template-repository-url] MyNewProject
cd MyNewProject

# Remove existing git history (optional)
rm -rf .git
git init
```

### Step 2: Customize Project Names

1. **Rename Solution and Projects:**
   ```bash
   # Rename solution file
   mv SOLID_Template.sln MyNewProject.sln
   
   # Rename project folders
   mv SOLID_Template.API MyNewProject.API
   mv SOLID_Template.Tests MyNewProject.Tests
   ```

2. **Update Project References:**
   - Open `MyNewProject.sln`
   - Update project paths and names
   - Update namespace references in all files

### Step 3: Build and Test

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests to verify everything works
dotnet test

# Expected output: All tests should pass ✅
```

### Step 4: Run the Application

```bash
# Run the API
dotnet run --project MyNewProject.API

# The API should start on http://localhost:5000
```

### Step 5: Verify Installation

Open your browser and visit:

- **API Root:** `http://localhost:5000`
- **Swagger UI:** `http://localhost:5000/swagger`
- **Health Check:** `http://localhost:5000/health`

## 🔧 Configuration

### Environment Configuration

The template supports different environments out of the box:

#### Development Environment

```json
// appsettings.Development.json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug"
    }
  }
}
```

#### Production Environment

```json
// appsettings.json  
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server;Database=MyNewProject;Trusted_Connection=true;"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information"
    }
  }
}
```

### Database Configuration

#### Development (In-Memory Database)

No setup required. The template uses Entity Framework In-Memory database for development.

#### Production (SQL Server)

1. **Update Connection String:**
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=your-server;Database=MyNewProject;Trusted_Connection=true;MultipleActiveResultSets=true"
     }
   }
   ```

2. **Create Database Migrations:**
   ```bash
   # Add Entity Framework tools (if not already installed)
   dotnet tool install --global dotnet-ef
   
   # Create initial migration
   dotnet ef migrations add InitialCreate --project MyNewProject.API
   
   # Update database
   dotnet ef database update --project MyNewProject.API
   ```

## 🧪 Testing Your Setup

### 1. Test Health Check

```bash
curl http://localhost:5000/health
# Expected: HTTP 200 OK
```

### 2. Test API Endpoints

```bash
# Get all products
curl http://localhost:5000/api/products

# Create a new product
curl -X POST http://localhost:5000/api/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Laptop Dell XPS",
    "sku": "DELL-XPS-001",
    "price": 1200.50,
    "stockQuantity": 15
  }'
```

### 3. Test Swagger UI

1. Open `http://localhost:5000/swagger`
2. Expand the **Products** controller
3. Try the **GET /api/products** endpoint
4. Execute and verify response

## 📁 Project Structure Overview

```
MyNewProject/
├── MyNewProject.API/              # 🌐 Web API Project
│   ├── Domain/                    # Business entities and interfaces
│   │   ├── Entities/              # Core business objects
│   │   └── Interfaces/            # Repository contracts
│   ├── Application/               # Business logic layer
│   │   ├── Services/              # Business logic implementation
│   │   ├── DTOs/                  # Data transfer objects
│   │   ├── Validators/            # Input validation rules
│   │   └── Mappings/              # AutoMapper configurations
│   ├── Infrastructure/            # Data access and external services
│   │   ├── Data/                  # Database context
│   │   └── Repositories/          # Data access implementation
│   └── Presentation/              # API controllers and configuration
│       ├── Controllers/           # API endpoints
│       ├── Extensions/            # Dependency injection setup
│       └── Middleware/            # Global exception handling
├── MyNewProject.Tests/            # 🧪 Unit Tests
└── docs/                          # 📚 Documentation
```

## 🔄 Next Steps

Now that your project is set up, here's what to do next:

### 1. Customize for Your Domain

- **Replace Sample Entities:** Update `Person` and `Order` with your business entities
- **Update DTOs:** Modify data transfer objects to match your API needs
- **Customize Validation:** Update FluentValidation rules for your business logic

### 2. Add Your First Feature

Follow the **[Adding Features Guide](./adding-features.md)** to add your first custom feature.

### 3. Set Up Your Development Workflow

- **Configure IDE:** Set up debugging and testing in your preferred IDE
- **Set Up Git:** Initialize your repository and set up branching strategy
- **Configure CI/CD:** Set up automated builds and deployments

### 4. Learn the Architecture

Read the **[Architecture Overview](../architecture/clean-architecture.md)** to understand the design principles.

## 🆘 Troubleshooting

### Common Issues

#### Build Errors

```bash
# Clear build artifacts
dotnet clean
dotnet restore
dotnet build
```

#### Port Already in Use

```bash
# Run on different port
dotnet run --project MyNewProject.API --urls "http://localhost:5001"
```

#### Database Connection Issues

- Verify connection string in `appsettings.json`
- Ensure SQL Server is running (for production)
- Check firewall settings

#### Package Version Conflicts

```bash
# Update all packages to latest compatible versions
dotnet list package --outdated
dotnet add package [PackageName] --version [Version]
```

### Getting Help

- **Check Documentation:** Review relevant guides in `/docs`
- **Review Examples:** Look at code examples in `/docs/examples`
- **Check Issues:** Review common patterns and solutions

## ✅ Verification Checklist

Before proceeding to development, ensure:

- [ ] Project builds without errors
- [ ] All tests pass
- [ ] API starts successfully
- [ ] Swagger UI loads and shows endpoints
- [ ] Health check returns HTTP 200
- [ ] Sample API endpoints respond correctly
- [ ] Logging appears in console and files
- [ ] Database connection works (if using SQL Server)

## 🎉 Success!

Congratulations! You now have a fully functional, production-ready Web API foundation. 

**Next Steps:**
1. **[Add Your First Feature](./adding-features.md)**
2. **[Understand the Architecture](../architecture/clean-architecture.md)**  
3. **[Learn Testing Strategies](./testing-guide.md)**

Happy coding! 🚀