using Serilog;
using SOLID_Template.Presentation.Extensions;
using SOLID_Template.Presentation.Middleware;

var builder = WebApplication.CreateBuilder(args);

try
{
    // Configure Serilog
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .CreateLogger();

    builder.Host.UseSerilog();

    Log.Information("Starting SOLID Template API");

    // Add services to the container
    builder.Services.AddControllers(options =>
    {
        // Automatic model validation
        options.ModelValidatorProviders.Clear();
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // Custom validation error response
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(e => new
                {
                    Field = x.Key,
                    Error = e.ErrorMessage
                }));

            var problemDetails = new Microsoft.AspNetCore.Mvc.ValidationProblemDetails(context.ModelState)
            {
                Title = "Validation Error",
                Detail = "One or more validation errors occurred.",
                Instance = context.HttpContext.Request.Path,
                Extensions = { { "traceId", context.HttpContext.TraceIdentifier } }
            };

            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(problemDetails)
            {
                ContentTypes = { "application/json" }
            };
        };
    });

    // Configure custom services following SOLID principles
    builder.Services.AddDatabase(builder.Configuration, builder.Environment);
    builder.Services.AddRepositories();
    builder.Services.AddApplicationServices();
    builder.Services.AddValidators();
    builder.Services.AddAutoMapperProfiles();

    // Add production-ready services
    builder.Services.AddHealthChecksConfiguration();
    builder.Services.AddCorsConfiguration(builder.Configuration, builder.Environment);
    builder.Services.AddResponseCompressionConfiguration();
    builder.Services.AddCachingConfiguration();

    // Add Swagger only in development
    if (builder.Environment.IsDevelopment())
    {
        builder.Services.AddSwaggerDocumentation();
    }

    var app = builder.Build();

    // Configure the HTTP request pipeline
    
    // Global exception handling (should be first)
    app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

    // Response compression
    app.UseResponseCompression();

    // CORS - use environment-specific policy
    if (app.Environment.IsDevelopment())
    {
        app.UseCors("Development");
    }
    else
    {
        app.UseCors("Production");
    }

    // Swagger only in development
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "SOLID Template API v1");
            c.RoutePrefix = "swagger"; // Changed from root to /swagger
            c.DocumentTitle = "SOLID Template API - Development";
        });
    }

    // Health checks
    app.MapHealthChecks("/health");

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    Log.Information("SOLID Template API started successfully");
    
    // Log URLs where the application is running
    var urls = builder.WebHost.GetSetting(WebHostDefaults.ServerUrlsKey) ?? "http://localhost:5000";
    Log.Information("Application is running on: {Urls}", urls);
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
