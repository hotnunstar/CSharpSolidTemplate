using Microsoft.AspNetCore.Mvc;
using SOLID_Template.Application.DTOs.Order;
using SOLID_Template.Application.Interfaces;
using SOLID_Template.Domain.Entities;

namespace SOLID_Template.Presentation.Controllers;

/// <summary>
/// Controller for operations related to Order entity
/// Implements REST API endpoints following REST patterns
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Gets an order by ID
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Order data</returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _orderService.GetByIdAsync(id);
        
        if (!result.Success)
            return NotFound(result);
            
        return Ok(result);
    }

    /// <summary>
    /// Gets all active orders
    /// </summary>
    /// <returns>List of orders</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _orderService.GetActiveAsync();
        return Ok(result);
    }

    /// <summary>
    /// Gets order by number
    /// </summary>
    /// <param name="number">Order number</param>
    /// <returns>Order data</returns>
    [HttpGet("by-number/{number}")]
    public async Task<IActionResult> GetByNumber(string number)
    {
        var result = await _orderService.GetByNumberAsync(number);
        
        if (!result.Success)
            return NotFound(result);
            
        return Ok(result);
    }

    /// <summary>
    /// Gets orders by status
    /// </summary>
    /// <param name="status">Order status</param>
    /// <returns>List of orders with specified status</returns>
    [HttpGet("by-status/{status:int}")]
    public async Task<IActionResult> GetByStatus(OrderStatus status)
    {
        var result = await _orderService.GetByStatusAsync(status);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new order
    /// </summary>
    /// <param name="createDto">Data for creation</param>
    /// <returns>Created order</returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto createDto)
    {
        var result = await _orderService.CreateAsync(createDto);
        
        if (!result.Success)
            return BadRequest(result);
            
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Updates an existing order
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="updateDto">Data for update</param>
    /// <returns>Updated order</returns>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrderDto updateDto)
    {
        if (id != updateDto.Id)
            return BadRequest("URL ID does not match object ID");
            
        var result = await _orderService.UpdateAsync(updateDto);
        
        if (!result.Success)
            return BadRequest(result);
            
        return Ok(result);
    }

    /// <summary>
    /// Deletes an order (soft delete)
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Operation result</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _orderService.DeleteAsync(id);
        
        if (!result.Success)
            return NotFound(result);
            
        return Ok(result);
    }

    /// <summary>
    /// Adds a product to an order
    /// </summary>
    /// <param name="addProductDto">Association data</param>
    /// <returns>Operation result</returns>
    [HttpPost("add-product")]
    public async Task<IActionResult> AddProductToOrder([FromBody] AddProductToOrderDto addProductDto)
    {
        var result = await _orderService.AddProductToOrderAsync(addProductDto);
        
        if (!result.Success)
            return BadRequest(result);
            
        return Ok(result);
    }

    /// <summary>
    /// Removes a product from an order
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="productId">Product ID</param>
    /// <returns>Operation result</returns>
    [HttpDelete("{orderId:guid}/products/{productId:guid}")]
    public async Task<IActionResult> RemoveProductFromOrder(Guid orderId, Guid productId)
    {
        var result = await _orderService.RemoveProductFromOrderAsync(orderId, productId);
        
        if (!result.Success)
            return BadRequest(result);
            
        return Ok(result);
    }

    /// <summary>
    /// Approves an order
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Operation result</returns>
    [HttpPatch("{id:guid}/approve")]
    public async Task<IActionResult> ApproveOrder(Guid id)
    {
        var result = await _orderService.ApproveOrderAsync(id);
        
        if (!result.Success)
            return BadRequest(result);
            
        return Ok(result);
    }

    /// <summary>
    /// Cancels an order
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Operation result</returns>
    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> CancelOrder(Guid id)
    {
        var result = await _orderService.CancelOrderAsync(id);
        
        if (!result.Success)
            return BadRequest(result);
            
        return Ok(result);
    }
}