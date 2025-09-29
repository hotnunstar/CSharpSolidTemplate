using SOLID_Template.Application.DTOs;
using SOLID_Template.Application.DTOs.Order;
using SOLID_Template.Domain.Entities;

namespace SOLID_Template.Application.Interfaces;

/// <summary>
/// Interface for Order entity services
/// Defines contracts for business operations following Interface Segregation Principle (SOLID)
/// </summary>
public interface IOrderService
{
    Task<ApiResponseDto<OrderDto>> GetByIdAsync(Guid id);
    Task<ApiResponseDto<IEnumerable<OrderSummaryDto>>> GetAllAsync();
    Task<ApiResponseDto<IEnumerable<OrderSummaryDto>>> GetActiveAsync();
    Task<ApiResponseDto<OrderDto>> CreateAsync(CreateOrderDto createDto);
    Task<ApiResponseDto<OrderDto>> UpdateAsync(UpdateOrderDto updateDto);
    Task<BaseResponseDto> DeleteAsync(Guid id);
    Task<ApiResponseDto<OrderDto>> GetByNumberAsync(string number);
    Task<ApiResponseDto<IEnumerable<OrderSummaryDto>>> GetByStatusAsync(OrderStatus status);
    Task<BaseResponseDto> AddProductToOrderAsync(AddProductToOrderDto addProductDto);
    Task<BaseResponseDto> RemoveProductFromOrderAsync(Guid orderId, Guid productId);
    Task<BaseResponseDto> ApproveOrderAsync(Guid id);
    Task<BaseResponseDto> CancelOrderAsync(Guid id);
}