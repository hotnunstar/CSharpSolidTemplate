using AutoMapper;
using SOLID_Template.Application.DTOs;
using SOLID_Template.Application.DTOs.Order;
using SOLID_Template.Domain.Entities;

namespace SOLID_Template.Application.Mappings;

/// <summary>
/// AutoMapper profile to configure mappings between entities and DTOs
/// Centralizes mapping configurations following Single Responsibility Principle (SOLID)
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateProductMaps();
        CreateOrderMaps();
    }

    private void CreateProductMaps()
    {
        // Product Entity -> ProductDto
        CreateMap<Product, ProductDto>();

        // CreateProductDto -> Product Entity
        CreateMap<CreateProductDto, Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.OrderProducts, opt => opt.Ignore());

        // UpdateProductDto -> Product Entity
        CreateMap<UpdateProductDto, Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.OrderProducts, opt => opt.Ignore());
    }

    private void CreateOrderMaps()
    {
        // Order Entity -> OrderDto
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.StatusDescription, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.GetProductCount()))
            .ForMember(dest => dest.TotalQuantity, opt => opt.MapFrom(src => src.GetTotalQuantity()))
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.CalculateTotalAmount()))
            .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.OrderProducts));

        // CreateOrderDto -> Order Entity
        CreateMap<CreateOrderDto, Order>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Amount, opt => opt.Ignore()) // Will be calculated
            .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => OrderStatus.Pending))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.OrderProducts, opt => opt.Ignore());

        // OrderProduct Entity -> OrderProductDetailDto
        CreateMap<OrderProduct, OrderProductDetailDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.ProductSku, opt => opt.MapFrom(src => src.Product.Sku))
            .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.GetTotalPrice()));

        // UpdateOrderDto -> Order Entity
        CreateMap<UpdateOrderDto, Order>()
            .ForMember(dest => dest.OrderDate, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.OrderProducts, opt => opt.Ignore());

        // Order Entity -> OrderSummaryDto
        CreateMap<Order, OrderSummaryDto>()
            .ForMember(dest => dest.StatusDescription, opt => opt.MapFrom(src => src.Status.ToString()));
    }
}