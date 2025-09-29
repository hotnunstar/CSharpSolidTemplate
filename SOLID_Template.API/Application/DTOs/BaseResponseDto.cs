namespace SOLID_Template.Application.DTOs;

/// <summary>
/// Base DTO for API responses
/// Standardizes response format following best practices
/// </summary>
public class BaseResponseDto
{
    public bool Success { get; set; } = true;
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
    
    public static BaseResponseDto SuccessResult(string message = "Operation completed successfully")
    {
        return new BaseResponseDto { Success = true, Message = message };
    }
    
    public static BaseResponseDto ErrorResult(string error)
    {
        return new BaseResponseDto 
        { 
            Success = false, 
            Message = "Operation failed",
            Errors = new List<string> { error }
        };
    }
    
    public static BaseResponseDto ErrorResult(List<string> errors)
    {
        return new BaseResponseDto 
        { 
            Success = false, 
            Message = "Operation failed",
            Errors = errors
        };
    }
}

/// <summary>
/// Base DTO for API responses with data
/// </summary>
/// <typeparam name="T">Type of returned data</typeparam>
public class ApiResponseDto<T> : BaseResponseDto
{
    public T? Data { get; set; }
    
    public static ApiResponseDto<T> SuccessResult(T data, string message = "Operation completed successfully")
    {
        return new ApiResponseDto<T> 
        { 
            Success = true, 
            Message = message,
            Data = data
        };
    }
    
    public new static ApiResponseDto<T> ErrorResult(string error)
    {
        return new ApiResponseDto<T> 
        { 
            Success = false, 
            Message = "Operation failed",
            Errors = new List<string> { error }
        };
    }
    
    public new static ApiResponseDto<T> ErrorResult(List<string> errors)
    {
        return new ApiResponseDto<T> 
        { 
            Success = false, 
            Message = "Operation failed",
            Errors = errors
        };
    }
}