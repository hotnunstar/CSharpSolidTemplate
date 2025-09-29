using AutoMapper;
using FluentValidation;
using SOLID_Template.Application.DTOs;
using SOLID_Template.Application.DTOs.Order;
using SOLID_Template.Application.Interfaces;
using SOLID_Template.Domain.Entities;
using SOLID_Template.Domain.Interfaces;

namespace SOLID_Template.Application.Services;

/// <summary>
/// Implementation of business services for Order entity
/// Follows SOLID principles: Single Responsibility, Open/Closed, Dependency Inversion
/// </summary>
public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateOrderDto> _createValidator;

    public OrderService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IMapper mapper,
        IValidator<CreateOrderDto> createValidator)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _mapper = mapper;
        _createValidator = createValidator;
    }

    public async Task<ApiResponseDto<OrderDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var order = await _orderRepository.GetWithProductsAsync(id);
            
            if (order == null || order.IsDeleted)
            {
                return ApiResponseDto<OrderDto>.ErrorResult("Order not found");
            }

            var orderDto = _mapper.Map<OrderDto>(order);
            return ApiResponseDto<OrderDto>.SuccessResult(orderDto);
        }
        catch (Exception ex)
        {
            return ApiResponseDto<OrderDto>.ErrorResult($"Internal error: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<IEnumerable<OrderSummaryDto>>> GetAllAsync()
    {
        try
        {
            var orders = await _orderRepository.GetAllAsync();
            var ordersDto = _mapper.Map<IEnumerable<OrderSummaryDto>>(orders);
            
            return ApiResponseDto<IEnumerable<OrderSummaryDto>>.SuccessResult(ordersDto);
        }
        catch (Exception ex)
        {
            return ApiResponseDto<IEnumerable<OrderSummaryDto>>.ErrorResult($"Internal error: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<IEnumerable<OrderSummaryDto>>> GetActiveAsync()
    {
        try
        {
            var orders = await _orderRepository.GetActiveAsync();
            var ordersDto = _mapper.Map<IEnumerable<OrderSummaryDto>>(orders);
            
            return ApiResponseDto<IEnumerable<OrderSummaryDto>>.SuccessResult(ordersDto);
        }
        catch (Exception ex)
        {
            return ApiResponseDto<IEnumerable<OrderSummaryDto>>.ErrorResult($"Internal error: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<OrderDto>> CreateAsync(CreateOrderDto createDto)
    {
        try
        {
            // Validation
            var validationResult = await _createValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ApiResponseDto<OrderDto>.ErrorResult(errors);
            }

            // Check if all products exist
            foreach (var productDto in createDto.Products)
            {
                var productExists = await _productRepository.ExistsAsync(productDto.ProductId);
                if (!productExists)
                {
                    return ApiResponseDto<OrderDto>.ErrorResult($"Product with ID {productDto.ProductId} not found");
                }
            }

            // Create order
            var order = _mapper.Map<Order>(createDto);
            var createdOrder = await _orderRepository.AddAsync(order);

            // Associate products with the order
            foreach (var productDto in createDto.Products)
            {
                createdOrder.OrderProducts.Add(new OrderProduct
                {
                    OrderId = createdOrder.Id,
                    ProductId = productDto.ProductId,
                    Quantity = productDto.Quantity,
                    UnitPrice = productDto.UnitPrice ?? 0m,
                    Discount = productDto.Discount
                });
            }

            var updatedOrder = await _orderRepository.UpdateAsync(createdOrder);
            var orderWithProducts = await _orderRepository.GetWithProductsAsync(updatedOrder.Id);
            var orderDto = _mapper.Map<OrderDto>(orderWithProducts);

            return ApiResponseDto<OrderDto>.SuccessResult(orderDto, "Order created successfully");
        }
        catch (Exception ex)
        {
            return ApiResponseDto<OrderDto>.ErrorResult($"Internal error: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<OrderDto>> UpdateAsync(UpdateOrderDto updateDto)
    {
        try
        {
            var existingOrder = await _orderRepository.GetByIdAsync(updateDto.Id);
            if (existingOrder == null || existingOrder.IsDeleted)
            {
                return ApiResponseDto<OrderDto>.ErrorResult("Order not found");
            }

            // Check if number already exists (excluding the order being updated)
            if (existingOrder.Number != updateDto.Number)
            {
                var numberExists = await _orderRepository.NumberExistsAsync(updateDto.Number);
                if (numberExists)
                {
                    return ApiResponseDto<OrderDto>.ErrorResult("This order number already exists");
                }
            }

            _mapper.Map(updateDto, existingOrder);
            var updatedOrder = await _orderRepository.UpdateAsync(existingOrder);
            var orderDto = _mapper.Map<OrderDto>(updatedOrder);

            return ApiResponseDto<OrderDto>.SuccessResult(orderDto, "Order updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponseDto<OrderDto>.ErrorResult($"Internal error: {ex.Message}");
        }
    }

    public async Task<BaseResponseDto> DeleteAsync(Guid id)
    {
        try
        {
            var exists = await _orderRepository.ExistsAsync(id);
            if (!exists)
            {
                return BaseResponseDto.ErrorResult("Order not found");
            }

            var success = await _orderRepository.DeleteAsync(id);
            if (success)
            {
                return BaseResponseDto.SuccessResult("Order deleted successfully");
            }
            
            return BaseResponseDto.ErrorResult("Error deleting order");
        }
        catch (Exception ex)
        {
            return BaseResponseDto.ErrorResult($"Internal error: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<OrderDto>> GetByNumberAsync(string number)
    {
        try
        {
            var order = await _orderRepository.GetByNumberAsync(number);
            
            if (order == null || order.IsDeleted)
            {
                return ApiResponseDto<OrderDto>.ErrorResult("Order not found");
            }

            var orderDto = _mapper.Map<OrderDto>(order);
            return ApiResponseDto<OrderDto>.SuccessResult(orderDto);
        }
        catch (Exception ex)
        {
            return ApiResponseDto<OrderDto>.ErrorResult($"Internal error: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<IEnumerable<OrderSummaryDto>>> GetByStatusAsync(OrderStatus status)
    {
        try
        {
            var orders = await _orderRepository.GetByStatusAsync(status);
            var ordersDto = _mapper.Map<IEnumerable<OrderSummaryDto>>(orders);
            
            return ApiResponseDto<IEnumerable<OrderSummaryDto>>.SuccessResult(ordersDto);
        }
        catch (Exception ex)
        {
            return ApiResponseDto<IEnumerable<OrderSummaryDto>>.ErrorResult($"Internal error: {ex.Message}");
        }
    }

    public async Task<BaseResponseDto> AddProductToOrderAsync(AddProductToOrderDto addProductDto)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(addProductDto.OrderId);
            if (order == null || order.IsDeleted)
            {
                return BaseResponseDto.ErrorResult("Order not found");
            }

            var productExists = await _productRepository.ExistsAsync(addProductDto.ProductId);
            if (!productExists)
            {
                return BaseResponseDto.ErrorResult("Product not found");
            }

            // Check if association already exists
            var associationExists = order.OrderProducts.Any(op => op.ProductId == addProductDto.ProductId);
            if (associationExists)
            {
                return BaseResponseDto.ErrorResult("Product is already associated with this order");
            }

            order.OrderProducts.Add(new OrderProduct
            {
                OrderId = addProductDto.OrderId,
                ProductId = addProductDto.ProductId,
                Quantity = addProductDto.Quantity,
                UnitPrice = addProductDto.UnitPrice ?? 0m,
                Discount = addProductDto.Discount
            });

            await _orderRepository.UpdateAsync(order);
            return BaseResponseDto.SuccessResult("Product added to order successfully");
        }
        catch (Exception ex)
        {
            return BaseResponseDto.ErrorResult($"Internal error: {ex.Message}");
        }
    }

    public async Task<BaseResponseDto> RemoveProductFromOrderAsync(Guid orderId, Guid productId)
    {
        try
        {
            var order = await _orderRepository.GetWithProductsAsync(orderId);
            if (order == null || order.IsDeleted)
            {
                return BaseResponseDto.ErrorResult("Order not found");
            }

            var association = order.OrderProducts.FirstOrDefault(op => op.ProductId == productId);
            if (association == null)
            {
                return BaseResponseDto.ErrorResult("Product is not associated with this order");
            }

            order.OrderProducts.Remove(association);
            await _orderRepository.UpdateAsync(order);
            
            return BaseResponseDto.SuccessResult("Product removed from order successfully");
        }
        catch (Exception ex)
        {
            return BaseResponseDto.ErrorResult($"Internal error: {ex.Message}");
        }
    }

    public async Task<BaseResponseDto> ApproveOrderAsync(Guid id)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null || order.IsDeleted)
            {
                return BaseResponseDto.ErrorResult("Order not found");
            }

            order.Approve();
            await _orderRepository.UpdateAsync(order);
            
            return BaseResponseDto.SuccessResult("Order approved successfully");
        }
        catch (Exception ex)
        {
            return BaseResponseDto.ErrorResult($"Internal error: {ex.Message}");
        }
    }

    public async Task<BaseResponseDto> CancelOrderAsync(Guid id)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null || order.IsDeleted)
            {
                return BaseResponseDto.ErrorResult("Order not found");
            }

            if (!order.CanCancel())
            {
                return BaseResponseDto.ErrorResult("Order cannot be canceled in current status");
            }

            order.Cancel();
            await _orderRepository.UpdateAsync(order);
            
            return BaseResponseDto.SuccessResult("Order canceled successfully");
        }
        catch (Exception ex)
        {
            return BaseResponseDto.ErrorResult($"Internal error: {ex.Message}");
        }
    }
}