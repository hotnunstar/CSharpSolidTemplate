using Microsoft.AspNetCore.Mvc;
using SOLID_Template.Application.DTOs;
using SOLID_Template.Application.Interfaces;

namespace SOLID_Template.Presentation.Controllers;

/// <summary>
/// Products API controller demonstrating Clean Architecture and SOLID principles
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Get all products
    /// </summary>
    /// <returns>List of products</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<ProductDto>>>> GetAll()
    {
        var result = await _productService.GetAllAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product details</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponseDto<ProductDto>>> GetById(Guid id)
    {
        var result = await _productService.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Get product by SKU
    /// </summary>
    /// <param name="sku">Product SKU</param>
    /// <returns>Product details</returns>
    [HttpGet("sku/{sku}")]
    public async Task<ActionResult<ApiResponseDto<ProductDto>>> GetBySku(string sku)
    {
        var result = await _productService.GetBySkuAsync(sku);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Get products by category
    /// </summary>
    /// <param name="category">Product category</param>
    /// <returns>List of products in category</returns>
    [HttpGet("category/{category}")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<ProductDto>>>> GetByCategory(string category)
    {
        var result = await _productService.GetByCategoryAsync(category);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get available products (in stock and active)
    /// </summary>
    /// <returns>List of available products</returns>
    [HttpGet("available")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<ProductDto>>>> GetAvailable()
    {
        var result = await _productService.GetAvailableProductsAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get products within price range
    /// </summary>
    /// <param name="minPrice">Minimum price</param>
    /// <param name="maxPrice">Maximum price</param>
    /// <returns>List of products in price range</returns>
    [HttpGet("price-range")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<ProductDto>>>> GetByPriceRange(
        [FromQuery] decimal minPrice, 
        [FromQuery] decimal maxPrice)
    {
        var result = await _productService.GetByPriceRangeAsync(minPrice, maxPrice);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get products with low stock
    /// </summary>
    /// <param name="threshold">Stock threshold (default: 10)</param>
    /// <returns>List of low stock products</returns>
    [HttpGet("low-stock")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<ProductDto>>>> GetLowStock([FromQuery] int threshold = 10)
    {
        var result = await _productService.GetLowStockProductsAsync(threshold);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    /// <param name="createProductDto">Product data</param>
    /// <returns>Created product</returns>
    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<ProductDto>>> Create([FromBody] CreateProductDto createProductDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _productService.CreateAsync(createProductDto);
        
        return result.Success 
            ? CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result)
            : BadRequest(result);
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="updateProductDto">Updated product data</param>
    /// <returns>Updated product</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponseDto<ProductDto>>> Update(Guid id, [FromBody] UpdateProductDto updateProductDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _productService.UpdateAsync(id, updateProductDto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Update product stock
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="updateStockDto">Stock update data</param>
    /// <returns>Updated product</returns>
    [HttpPatch("{id}/stock")]
    public async Task<ActionResult<ApiResponseDto<ProductDto>>> UpdateStock(Guid id, [FromBody] UpdateStockDto updateStockDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _productService.UpdateStockAsync(id, updateStockDto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete a product (soft delete)
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Deletion confirmation</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponseDto<bool>>> Delete(Guid id)
    {
        var result = await _productService.DeleteAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
