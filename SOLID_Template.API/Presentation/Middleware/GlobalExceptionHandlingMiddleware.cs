using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace SOLID_Template.Presentation.Middleware;

/// <summary>
/// Global exception handling middleware following SOLID principles
/// Provides centralized error handling and standardized API responses
/// </summary>
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next, 
        ILogger<GlobalExceptionHandlingMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Unhandled exception occurred. RequestPath: {RequestPath}, Method: {Method}, UserAgent: {UserAgent}, TraceId: {TraceId}", 
                context.Request.Path, 
                context.Request.Method,
                context.Request.Headers.UserAgent.FirstOrDefault(),
                context.TraceIdentifier);
                
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ProblemDetails
        {
            Instance = context.Request.Path,
            Extensions = { { "traceId", context.TraceIdentifier } }
        };

        switch (exception)
        {
            case ArgumentNullException argNull:
                response.Status = (int)HttpStatusCode.BadRequest;
                response.Title = "Bad Request";
                response.Detail = "One or more required parameters are missing.";
                response.Extensions.Add("parameterName", argNull.ParamName);
                break;

            case ArgumentException arg:
                response.Status = (int)HttpStatusCode.BadRequest;
                response.Title = "Bad Request";
                response.Detail = _environment.IsDevelopment() ? arg.Message : "Invalid argument provided.";
                if (!string.IsNullOrEmpty(arg.ParamName))
                    response.Extensions.Add("parameterName", arg.ParamName);
                break;

            case KeyNotFoundException:
                response.Status = (int)HttpStatusCode.NotFound;
                response.Title = "Resource Not Found";
                response.Detail = "The requested resource was not found.";
                break;

            case UnauthorizedAccessException:
                response.Status = (int)HttpStatusCode.Unauthorized;
                response.Title = "Unauthorized";
                response.Detail = "You are not authorized to access this resource.";
                break;

            case InvalidOperationException invalidOp:
                response.Status = (int)HttpStatusCode.BadRequest;
                response.Title = "Invalid Operation";
                response.Detail = _environment.IsDevelopment() ? invalidOp.Message : "The operation cannot be completed.";
                break;

            case TaskCanceledException:
                response.Status = (int)HttpStatusCode.RequestTimeout;
                response.Title = "Request Timeout";
                response.Detail = "The request has timed out.";
                break;

            case FluentValidation.ValidationException validationEx:
                response.Status = (int)HttpStatusCode.BadRequest;
                response.Title = "Validation Error";
                response.Detail = "One or more validation errors occurred.";
                response.Extensions.Add("errors", validationEx.Errors.Select(e => new { 
                    Field = e.PropertyName, 
                    Error = e.ErrorMessage 
                }));
                break;

            default:
                response.Status = (int)HttpStatusCode.InternalServerError;
                response.Title = "Internal Server Error";
                response.Detail = _environment.IsDevelopment() 
                    ? $"{exception.Message}\n{exception.StackTrace}" 
                    : "An error occurred while processing your request.";
                break;
        }

        context.Response.StatusCode = response.Status.Value;

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment()
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}