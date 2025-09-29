using FluentValidation;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using SOLID_Template.Application.Interfaces;
using SOLID_Template.Application.Services;
using SOLID_Template.Application.Validators;
using SOLID_Template.Domain.Interfaces;
using SOLID_Template.Infrastructure.Data;
using SOLID_Template.Infrastructure.Repositories;

namespace SOLID_Template.Presentation.Extensions;

/// <summary>
/// Extensions for dependency injection configuration
/// Centralizes service registration following Dependency Inversion Principle (SOLID)
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers application services
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Application Services
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();

        return services;
    }

    /// <summary>
    /// Registers repositories
    /// </summary>
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        return services;
    }

    /// <summary>
    /// Configures Entity Framework with environment-specific databases
    /// </summary>
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            // Development: InMemory Database
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("SOLID_Template_Dev"));
        }
        else
        {
            // Production: SQL Server (easily replaceable with other providers)
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
        }

        return services;
    }

    /// <summary>
    /// Registers validators
    /// </summary>
    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateProductValidator>();
        
        return services;
    }

    /// <summary>
    /// Configures AutoMapper
    /// </summary>
    public static IServiceCollection AddAutoMapperProfiles(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(Program));
        
        return services;
    }

    /// <summary>
    /// Configures Swagger/OpenAPI
    /// </summary>
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() 
            { 
                Title = "SOLID Template API", 
                Version = "v1",
                Description = "Web API Template following SOLID principles"
            });

            // Include XML comments
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }

    /// <summary>
    /// Configures Health Checks
    /// </summary>
    public static IServiceCollection AddHealthChecksConfiguration(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();

        return services;
    }

    /// <summary>
    /// Configures CORS with environment-specific policies
    /// </summary>
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddCors(options =>
        {
            // Development policy - more permissive
            options.AddPolicy("Development", policy =>
            {
                var corsSettings = configuration.GetSection("CORS");
                var allowAnyOrigin = corsSettings.GetValue<bool>("AllowAnyOrigin");
                
                if (allowAnyOrigin)
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                }
                else
                {
                    var allowedOrigins = corsSettings.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                }
            });

            // Production policy - more restrictive
            options.AddPolicy("Production", policy =>
            {
                var corsSettings = configuration.GetSection("CORS");
                var allowedOrigins = corsSettings.GetSection("AllowedOrigins").Get<string[]>() 
                    ?? new[] { "https://yourdomain.com" };
                
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials()
                      .SetIsOriginAllowedToAllowWildcardSubdomains();
            });
        });

        return services;
    }

    /// <summary>
    /// Configures Response Compression
    /// </summary>
    public static IServiceCollection AddResponseCompressionConfiguration(this IServiceCollection services)
    {
        services.AddResponseCompression(opts =>
        {
            opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                new[] { "application/json" });
        });

        return services;
    }

    /// <summary>
    /// Configures Memory Caching
    /// </summary>
    public static IServiceCollection AddCachingConfiguration(this IServiceCollection services)
    {
        services.AddMemoryCache();
        return services;
    }
}