using SOLID_Template.Application.DTOs;

namespace SOLID_Template.Application.Interfaces;

/// <summary>
/// Product service interface defining business operations
/// </summary>
public interface IProductService
{
    Task<ApiResponseDto<IEnumerable<ProductDto>>> GetAllAsync();
    Task<ApiResponseDto<ProductDto>> GetByIdAsync(Guid id);
    Task<ApiResponseDto<ProductDto>> GetBySkuAsync(string sku);
    Task<ApiResponseDto<IEnumerable<ProductDto>>> GetByCategoryAsync(string category);
    Task<ApiResponseDto<IEnumerable<ProductDto>>> GetAvailableProductsAsync();
    Task<ApiResponseDto<IEnumerable<ProductDto>>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice);
    Task<ApiResponseDto<IEnumerable<ProductDto>>> GetLowStockProductsAsync(int threshold = 10);
    Task<ApiResponseDto<ProductDto>> CreateAsync(CreateProductDto createProductDto);
    Task<ApiResponseDto<ProductDto>> UpdateAsync(Guid id, UpdateProductDto updateProductDto);
    Task<ApiResponseDto<ProductDto>> UpdateStockAsync(Guid id, UpdateStockDto updateStockDto);
    Task<BaseResponseDto> DeleteAsync(Guid id);
}