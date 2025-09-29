using AutoMapper;
using SOLID_Template.Application.DTOs;
using SOLID_Template.Application.Interfaces;
using SOLID_Template.Domain.Entities;
using SOLID_Template.Domain.Interfaces;

namespace SOLID_Template.Application.Services;

/// <summary>
/// Product service implementation following Single Responsibility Principle
/// Handles all product-related business operations
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public ProductService(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponseDto<IEnumerable<ProductDto>>> GetAllAsync()
    {
        try
        {
            var products = await _productRepository.GetAllAsync();
            var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);

            // Set calculated properties
            foreach (var dto in productDtos)
            {
                var product = products.First(p => p.Id == dto.Id);
                dto.IsAvailable = product.IsAvailable();
            }

            return ApiResponseDto<IEnumerable<ProductDto>>.SuccessResult(productDtos);
        }
        catch (Exception ex)
        {
            return ApiResponseDto<IEnumerable<ProductDto>>.ErrorResult($"Error retrieving products: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<ProductDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return ApiResponseDto<ProductDto>.ErrorResult("Product not found");
            }

            var productDto = _mapper.Map<ProductDto>(product);
            productDto.IsAvailable = product.IsAvailable();

            return ApiResponseDto<ProductDto>.SuccessResult(productDto);
        }
        catch (Exception ex)
        {
            return ApiResponseDto<ProductDto>.ErrorResult($"Error retrieving product: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<ProductDto>> GetBySkuAsync(string sku)
    {
        try
        {
            var product = await _productRepository.GetBySkuAsync(sku);
            if (product == null)
            {
                return ApiResponseDto<ProductDto>.ErrorResult("Product not found");
            }

            var productDto = _mapper.Map<ProductDto>(product);
            productDto.IsAvailable = product.IsAvailable();

            return ApiResponseDto<ProductDto>.SuccessResult(productDto);
        }
        catch (Exception ex)
        {
            return ApiResponseDto<ProductDto>.ErrorResult($"Error retrieving product: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<IEnumerable<ProductDto>>> GetByCategoryAsync(string category)
    {
        try
        {
            var products = await _productRepository.GetByCategoryAsync(category);
            var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);

            foreach (var dto in productDtos)
            {
                var product = products.First(p => p.Id == dto.Id);
                dto.IsAvailable = product.IsAvailable();
            }

            return ApiResponseDto<IEnumerable<ProductDto>>.SuccessResult(productDtos);
        }
        catch (Exception ex)
        {
            return ApiResponseDto<IEnumerable<ProductDto>>.ErrorResult($"Error retrieving products by category: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<IEnumerable<ProductDto>>> GetAvailableProductsAsync()
    {
        try
        {
            var products = await _productRepository.GetAvailableProductsAsync();
            var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);

            foreach (var dto in productDtos)
            {
                dto.IsAvailable = true; // All these are available by definition
            }

            return ApiResponseDto<IEnumerable<ProductDto>>.SuccessResult(productDtos);
        }
        catch (Exception ex)
        {
            return ApiResponseDto<IEnumerable<ProductDto>>.ErrorResult($"Error retrieving available products: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<IEnumerable<ProductDto>>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        try
        {
            if (minPrice < 0 || maxPrice < 0 || minPrice > maxPrice)
            {
                return ApiResponseDto<IEnumerable<ProductDto>>.ErrorResult("Invalid price range");
            }

            var products = await _productRepository.GetByPriceRangeAsync(minPrice, maxPrice);
            var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);

            foreach (var dto in productDtos)
            {
                var product = products.First(p => p.Id == dto.Id);
                dto.IsAvailable = product.IsAvailable();
            }

            return ApiResponseDto<IEnumerable<ProductDto>>.SuccessResult(productDtos);
        }
        catch (Exception ex)
        {
            return ApiResponseDto<IEnumerable<ProductDto>>.ErrorResult($"Error retrieving products by price range: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<IEnumerable<ProductDto>>> GetLowStockProductsAsync(int threshold = 10)
    {
        try
        {
            var products = await _productRepository.GetLowStockProductsAsync(threshold);
            var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);

            foreach (var dto in productDtos)
            {
                var product = products.First(p => p.Id == dto.Id);
                dto.IsAvailable = product.IsAvailable();
            }

            return ApiResponseDto<IEnumerable<ProductDto>>.SuccessResult(productDtos);
        }
        catch (Exception ex)
        {
            return ApiResponseDto<IEnumerable<ProductDto>>.ErrorResult($"Error retrieving low stock products: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<ProductDto>> CreateAsync(CreateProductDto createProductDto)
    {
        try
        {
            // Check if SKU already exists
            var skuExists = !await _productRepository.IsSkuUniqueAsync(createProductDto.Sku);
            if (skuExists)
            {
                return ApiResponseDto<ProductDto>.ErrorResult("A product with this SKU already exists");
            }

            var product = _mapper.Map<Product>(createProductDto);

            // Validate business rules
            if (!product.IsSkuValid())
            {
                return ApiResponseDto<ProductDto>.ErrorResult("Invalid SKU format");
            }

            if (product.Price < 0)
            {
                return ApiResponseDto<ProductDto>.ErrorResult("Price cannot be negative");
            }

            if (product.StockQuantity < 0)
            {
                return ApiResponseDto<ProductDto>.ErrorResult("Stock quantity cannot be negative");
            }

            var createdProduct = await _productRepository.AddAsync(product);
            var productDto = _mapper.Map<ProductDto>(createdProduct);
            productDto.IsAvailable = createdProduct.IsAvailable();

            return ApiResponseDto<ProductDto>.SuccessResult(productDto, "Product created successfully");
        }
        catch (Exception ex)
        {
            return ApiResponseDto<ProductDto>.ErrorResult($"Error creating product: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<ProductDto>> UpdateAsync(Guid id, UpdateProductDto updateProductDto)
    {
        try
        {
            var existingProduct = await _productRepository.GetByIdAsync(id);
            if (existingProduct == null)
            {
                return ApiResponseDto<ProductDto>.ErrorResult("Product not found");
            }

            // Check if SKU is unique (excluding current product)
            var skuUnique = await _productRepository.IsSkuUniqueAsync(updateProductDto.Sku, id);
            if (!skuUnique)
            {
                return ApiResponseDto<ProductDto>.ErrorResult("A product with this SKU already exists");
            }

            // Update properties
            existingProduct.Name = updateProductDto.Name;
            existingProduct.Description = updateProductDto.Description;
            existingProduct.Price = updateProductDto.Price;
            existingProduct.Sku = updateProductDto.Sku;
            existingProduct.StockQuantity = updateProductDto.StockQuantity;
            existingProduct.Category = updateProductDto.Category;
            existingProduct.IsActive = updateProductDto.IsActive;

            // Validate business rules
            if (!existingProduct.IsSkuValid())
            {
                return ApiResponseDto<ProductDto>.ErrorResult("Invalid SKU format");
            }

            if (existingProduct.Price < 0)
            {
                return ApiResponseDto<ProductDto>.ErrorResult("Price cannot be negative");
            }

            if (existingProduct.StockQuantity < 0)
            {
                return ApiResponseDto<ProductDto>.ErrorResult("Stock quantity cannot be negative");
            }

            var updatedProduct = await _productRepository.UpdateAsync(existingProduct);
            var productDto = _mapper.Map<ProductDto>(updatedProduct);
            productDto.IsAvailable = updatedProduct.IsAvailable();

            return ApiResponseDto<ProductDto>.SuccessResult(productDto, "Product updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponseDto<ProductDto>.ErrorResult($"Error updating product: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<ProductDto>> UpdateStockAsync(Guid id, UpdateStockDto updateStockDto)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return ApiResponseDto<ProductDto>.ErrorResult("Product not found");
            }

            var newStock = updateStockDto.Operation.ToLower() switch
            {
                "add" => product.StockQuantity + updateStockDto.Quantity,
                "subtract" => product.StockQuantity - updateStockDto.Quantity,
                _ => throw new ArgumentException("Invalid operation. Use 'add' or 'subtract'")
            };

            if (newStock < 0)
            {
                return ApiResponseDto<ProductDto>.ErrorResult("Insufficient stock for this operation");
            }

            product.StockQuantity = newStock;
            var updatedProduct = await _productRepository.UpdateAsync(product);
            
            var productDto = _mapper.Map<ProductDto>(updatedProduct);
            productDto.IsAvailable = updatedProduct.IsAvailable();

            return ApiResponseDto<ProductDto>.SuccessResult(productDto, "Stock updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponseDto<ProductDto>.ErrorResult($"Error updating stock: {ex.Message}");
        }
    }

    public async Task<BaseResponseDto> DeleteAsync(Guid id)
    {
        try
        {
            var exists = await _productRepository.ExistsAsync(id);
            if (!exists)
            {
                return BaseResponseDto.ErrorResult("Product not found");
            }

            await _productRepository.DeleteAsync(id);
            return BaseResponseDto.SuccessResult("Product deleted successfully");
        }
        catch (Exception ex)
        {
            return BaseResponseDto.ErrorResult($"Error deleting product: {ex.Message}");
        }
    }
}
