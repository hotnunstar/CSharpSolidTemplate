using Microsoft.EntityFrameworkCore;
using SOLID_Template.Infrastructure.Data;

namespace SOLID_Template.Tests.Helpers;

/// <summary>
/// Helper class for creating test database contexts
/// Follows Single Responsibility Principle - only responsible for test context creation
/// </summary>
public static class TestDbContextHelper
{
    /// <summary>
    /// Creates a new in-memory database context for testing
    /// </summary>
    /// <param name="databaseName">Name of the test database (should be unique per test)</param>
    /// <returns>ApplicationDbContext configured for testing</returns>
    public static ApplicationDbContext CreateInMemoryContext(string? databaseName = null)
    {
        databaseName ??= Guid.NewGuid().ToString();
        
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName)
            .EnableSensitiveDataLogging() // For better test debugging
            .Options;

        var context = new ApplicationDbContext(options);
        
        // Ensure database is created
        context.Database.EnsureCreated();
        
        return context;
    }

    /// <summary>
    /// Creates a context and ensures it's cleaned up after use
    /// </summary>
    /// <param name="testAction">Action to execute with the context</param>
    /// <param name="databaseName">Optional database name</param>
    public static void WithCleanContext(Action<ApplicationDbContext> testAction, string? databaseName = null)
    {
        using var context = CreateInMemoryContext(databaseName);
        testAction(context);
        context.Database.EnsureDeleted();
    }

    /// <summary>
    /// Creates a context for async operations and ensures cleanup
    /// </summary>
    /// <param name="testAction">Async action to execute with the context</param>
    /// <param name="databaseName">Optional database name</param>
    public static async Task WithCleanContextAsync(Func<ApplicationDbContext, Task> testAction, string? databaseName = null)
    {
        using var context = CreateInMemoryContext(databaseName);
        await testAction(context);
        await context.Database.EnsureDeletedAsync();
    }
}